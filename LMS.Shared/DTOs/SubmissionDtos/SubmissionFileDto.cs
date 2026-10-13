namespace LMS.Shared.DTOs.SubmissionDtos;


/// <summary>
/// One submitted file. The id is used for the download lin.
/// </summary>
public sealed record SubmissionFileDto(Guid Id, string Name);
