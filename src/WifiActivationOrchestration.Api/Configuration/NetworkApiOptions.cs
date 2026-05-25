using System.ComponentModel.DataAnnotations;

namespace WifiActivationOrchestration.Api.Configuration;

public sealed class NetworkApiOptions
{
    public const string SectionName = "NetworkApis";

    [Required]
    [Url]
    public string InfrastructureBaseUrl { get; set; } = string.Empty;

    [Required]
    [Url]
    public string ControllerBaseUrl { get; set; } = string.Empty;

    [Required]
    public string SpeedProfilesPath { get; set; } = string.Empty;

    [Required]
    public string ActivationPath { get; set; } = string.Empty;

    [Range(1, 60)]
    public int TimeoutSeconds { get; set; } = 10;
}