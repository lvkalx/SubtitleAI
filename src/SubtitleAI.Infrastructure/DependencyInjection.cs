using Azure.Storage.Blobs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AzureStorage;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubtitleAI.Application.Auth;
using SubtitleAI.Application.Jobs;
using SubtitleAI.Application.Storage;
using SubtitleAI.Application.Summarization;
using SubtitleAI.Application.Transcription;
using SubtitleAI.Infrastructure.Audio;
using SubtitleAI.Infrastructure.Auth;
using SubtitleAI.Infrastructure.Jobs;
using SubtitleAI.Infrastructure.Persistence;
using SubtitleAI.Infrastructure.Storage;
using SubtitleAI.Infrastructure.Summarization;
using SubtitleAI.Infrastructure.Transcription;

namespace SubtitleAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(config.GetConnectionString("Postgres")));

        services.Configure<JwtOptions>(config.GetSection("Jwt"));
        var adminOptions = config.GetSection("Admin").Get<AdminOptions>() ?? new AdminOptions();
        services.AddSingleton(adminOptions);
        services.Configure<EmailOptions>(config.GetSection("Email"));
        var emailOptions = config.GetSection("Email").Get<EmailOptions>() ?? new EmailOptions();
        services.AddSingleton(emailOptions);
        services.AddSingleton<IEmailSender, SmtpEmailSender>();

        services.AddIdentityCore<ApplicationUser>(o =>
        {
            o.User.RequireUniqueEmail = true;
            o.SignIn.RequireConfirmedEmail = emailOptions.RequireConfirmedEmail;
            o.Password.RequiredLength = 8;
            o.Password.RequireNonAlphanumeric = false;
        }).AddEntityFrameworkStores<AppDbContext>()
          .AddTokenProvider<PasswordResetTokenProvider>("Default");

        services.AddScoped<IAuthService, AuthService>();

        services.Configure<StorageOptions>(config.GetSection("Storage"));
        var storageConnection = config["Storage:ConnectionString"];
        var storageContainer = config["Storage:Container"];
        var blobService = new BlobServiceClient(storageConnection);
        services.AddSingleton(blobService);
        if (!string.IsNullOrWhiteSpace(storageConnection) && !string.IsNullOrWhiteSpace(storageContainer))
        {
            var keysBlob = CloudStorageAccount.Parse(storageConnection)
                .CreateCloudBlobClient()
                .GetContainerReference(storageContainer)
                .GetBlockBlobReference("system/data-protection-keys.xml");
            services.AddDataProtection().PersistKeysToAzureBlobStorage(keysBlob);
        }
        services.AddScoped<IFileStorage, BlobFileStorage>();
        services.AddScoped<IJobService, JobService>();
        services.Configure<WhisperSettings>(config.GetSection("Whisper"));
        services.Configure<JobProcessingOptions>(config.GetSection("Jobs"));
        services.AddSingleton<IJobQueue, ChannelJobQueue>();
        services.AddSingleton<IJobCancellationRegistry, JobCancellationRegistry>();
        services.AddSingleton<IAudioExtractor, FfmpegAudioExtractor>();
        services.AddTransient<ITranscriptionService, WhisperTranscriptionService>();

        // Register the LLM provider only when its required configuration is present.
        // Without an ILlmClient, summarization is disabled but jobs still complete successfully.
        services.Configure<SummaryOptions>(config.GetSection("Summary"));
        services.Configure<AzureOpenAiOptions>(config.GetSection("AzureOpenAI"));
        if (!string.IsNullOrWhiteSpace(config["AzureOpenAI:Endpoint"]) &&
            !string.IsNullOrWhiteSpace(config["AzureOpenAI:Deployment"]))
            services.AddSingleton<ILlmClient, AzureOpenAiLlmClient>();
        services.AddSingleton<ISummaryService>(sp =>
            sp.GetService<ILlmClient>() is { } llm
                ? new MapReduceSummaryService(
                    llm,
                    sp.GetRequiredService<IOptions<SummaryOptions>>(),
                    sp.GetRequiredService<ILogger<MapReduceSummaryService>>())
                : new DisabledSummaryService());

        services.AddHostedService<JobProcessor>();

        return services;
    }
}
