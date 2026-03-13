namespace AuraFlow.Core.Services;

/// <summary>
/// Request model for generation operations
/// </summary>
public class GenerationRequest
{
    /// <summary>
    /// The text prompt for generation
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// The model to use for generation
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Type of content to generate (image or video)
    /// </summary>
    public GenerationType Type { get; set; } = GenerationType.Image;

    /// <summary>
    /// Generation options and parameters
    /// </summary>
    public GenerationOptions Options { get; set; } = new();

    /// <summary>
    /// Optional user ID for tracking
    /// </summary>
    public string? UserId { get; set; }
}

/// <summary>
/// Options for generation parameters
/// </summary>
public class GenerationOptions
{
    /// <summary>
    /// Width of the output in pixels (default: 1024)
    /// </summary>
    public int Width { get; set; } = 1024;

    /// <summary>
    /// Height of the output in pixels (default: 1024)
    /// </summary>
    public int Height { get; set; } = 1024;

    /// <summary>
    /// Number of inference steps (default: 30)
    /// </summary>
    public int Steps { get; set; } = 30;

    /// <summary>
    /// Guidance scale for the model (default: 7.5)
    /// </summary>
    public double GuidanceScale { get; set; } = 7.5;

    /// <summary>
    /// Random seed for reproducibility (-1 for random, default: -1)
    /// </summary>
    public long Seed { get; set; } = -1;

    /// <summary>
    /// Negative prompt to exclude elements (optional)
    /// </summary>
    public string? NegativePrompt { get; set; }

    /// <summary>
    /// Number of images/videos to generate (default: 1)
    /// </summary>
    public int BatchSize { get; set; } = 1;
}

/// <summary>
/// Result model for generation operations
/// </summary>
public class GenerationResult
{
    /// <summary>
    /// Unique task identifier
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the generation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if failed (optional)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// List of generated file paths or URLs
    /// </summary>
    public IEnumerable<string> OutputFiles { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Time taken for generation in milliseconds
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Model used for generation
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Generation type (image or video)
    /// </summary>
    public GenerationType Type { get; set; }

    /// <summary>
    /// Timestamp when generation completed
    /// </summary>
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Progress information for a generation task
/// </summary>
public class GenerationProgress
{
    /// <summary>
    /// Unique task identifier
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the task
    /// </summary>
    public GenerationStatus Status { get; set; }

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public int ProgressPercent { get; set; }

    /// <summary>
    /// Current step number
    /// </summary>
    public int CurrentStep { get; set; }

    /// <summary>
    /// Total steps expected
    /// </summary>
    public int TotalSteps { get; set; }

    /// <summary>
    /// Current node being executed (for ComfyUI)
    /// </summary>
    public string? CurrentNode { get; set; }

    /// <summary>
    /// Timestamp when task started
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Estimated time remaining in milliseconds
    /// </summary>
    public long? EstimatedRemainingMs { get; set; }
}

/// <summary>
/// Available model information for generation
/// </summary>
public class GenerationModel
{
    /// <summary>
    /// Unique model identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the model
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of content the model generates
    /// </summary>
    public GenerationType Type { get; set; }

    /// <summary>
    /// Model description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Default dimensions for this model
    /// </summary>
    public (int Width, int Height)? DefaultDimensions { get; set; }
}

/// <summary>
/// Generation type enumeration
/// </summary>
public enum GenerationType
{
    Image = 0,
    Video = 1
}

/// <summary>
/// Generation status enumeration
/// </summary>
public enum GenerationStatus
{
    Pending = 0,
    Queued = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}
