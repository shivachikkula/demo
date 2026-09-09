using DistrictPortal.Api.Data.Entities;

namespace DistrictPortal.Api.Data.Seed;

/// <summary>
/// Seeds the database with the same demo data the Angular frontend ships as mock data,
/// so the API and the UI agree on record counts, names, and statuses out of the box.
/// </summary>
public static class SeedData
{
    private const string SchoolYear = "SY2526";

    public static void EnsureSeeded(AppDbContext db)
    {
        if (db.Leas.Any())
        {
            return;
        }

        db.Leas.AddRange(BuildLeas());
        db.Notifications.AddRange(BuildNotifications());
        db.SaveChanges();
    }

    private static IEnumerable<Lea> BuildLeas()
    {
        yield return BuildDcps();
        yield return BuildKippDc();
        yield return BuildFriendshipPcs();
        yield return BuildUdcCc();
    }

    private static Lea BuildDcps()
    {
        var lea = new Lea
        {
            Id = "dcps",
            Name = "DCPS",
            OrgLabel = "DCPS",
            DisplayName = "District of Columbia Public Schools",
        };

        var discipline = NewCollection(lea.Id, "discipline", "Discipline", new(2026, 9, 15), isActive: true);
        AddSubmission(
            discipline,
            "DCPS_Enrollment_Fall2026_v1.xlsx",
            new DateTimeOffset(2026, 8, 6, 0, 0, 0, TimeSpan.Zero),
            sizeBytes: 2_048_000,
            total: 4751,
            passed: 4600,
            failed: 100,
            warning: 51,
            status: SubmissionStatus.Failed);
        AddSubmission(
            discipline,
            "DCPS_Enrollment_Fall2026_v2.xlsx",
            new DateTimeOffset(2026, 8, 6, 1, 0, 0, TimeSpan.Zero),
            sizeBytes: 2_048_500,
            total: 4751,
            passed: 4650,
            failed: 60,
            warning: 41,
            status: SubmissionStatus.Failed);
        AddSubmission(
            discipline,
            "DCPS_Enrollment_Fall2026_v3.xlsx",
            new DateTimeOffset(2026, 8, 10, 14, 32, 0, TimeSpan.Zero),
            sizeBytes: 2_150_000,
            total: 4908,
            passed: 4822,
            failed: 54,
            warning: 32,
            status: SubmissionStatus.Passed);

        var course = NewCollection(lea.Id, "course", "Course", new(2026, 8, 19), isActive: true);
        AddSubmission(
            course,
            "DCPS_Course_Fall2026_v1.xlsx",
            new DateTimeOffset(2026, 8, 12, 9, 14, 0, TimeSpan.Zero),
            sizeBytes: 980_000,
            total: 2140,
            passed: null,
            failed: null,
            warning: null,
            status: SubmissionStatus.Processing);

        var clsdLea = NewCollection(lea.Id, "clsd-lea", "CLSD-LEA", new(2025, 8, 25), isActive: true);
        clsdLea.NoSubmissionStatus = "overdue";
        clsdLea.NoSubmissionStatusLabel = "Overdue";

        var coursePhaseII = NewCollection(lea.Id, "course-phase-ii", "Course phase II", new(2022, 12, 9), isActive: true);
        coursePhaseII.NoSubmissionStatus = "not-started";
        coursePhaseII.NoSubmissionStatusLabel = "Not started";

        lea.Collections.AddRange(
        [
            discipline,
            course,
            clsdLea,
            coursePhaseII,
            NewCollection(lea.Id, "staff", "Staff", new(2025, 6, 30), isActive: false),
            NewCollection(lea.Id, "assessment", "Assessment", new(2025, 5, 15), isActive: false),
            NewCollection(lea.Id, "graduation", "Graduation", new(2025, 4, 1), isActive: false),
            NewCollection(lea.Id, "attendance", "Attendance", new(2025, 3, 10), isActive: false),
            NewCollection(lea.Id, "enrollment-fall", "Enrollment Fall", new(2024, 10, 5), isActive: false),
            NewCollection(lea.Id, "special-ed", "Special Education", new(2024, 9, 20), isActive: false),
            NewCollection(lea.Id, "title-i", "Title I", new(2024, 8, 1), isActive: false),
        ]);

        return lea;
    }

    private static Lea BuildKippDc()
    {
        var lea = new Lea
        {
            Id = "kipp-dc",
            Name = "KIPP DC",
            OrgLabel = "KIPP DC",
            DisplayName = "KIPP DC Public Charter Schools",
        };

        var enrollment = NewCollection(lea.Id, "enrollment", "Enrollment", new(2026, 8, 5), isActive: true);
        AddSubmission(
            enrollment,
            "Enrollment_v1.xlsx",
            new DateTimeOffset(2026, 7, 30, 0, 0, 0, TimeSpan.Zero),
            sizeBytes: 630_000,
            total: 1540,
            passed: 1180,
            failed: 320,
            warning: 40,
            status: SubmissionStatus.Failed);
        AddSubmission(
            enrollment,
            "KIPP_Enrollment_Fall2026_v2.xlsx",
            new DateTimeOffset(2026, 8, 5, 10, 15, 0, TimeSpan.Zero),
            sizeBytes: 640_000,
            total: 1560,
            passed: 1200,
            failed: 310,
            warning: 50,
            status: SubmissionStatus.Failed);

        var discipline = NewCollection(lea.Id, "discipline-kipp", "Discipline", new(2026, 8, 12), isActive: true);
        AddSubmission(
            discipline,
            "KIPP_Discipline_Fall2026_v1.xlsx",
            new DateTimeOffset(2026, 8, 12, 13, 5, 0, TimeSpan.Zero),
            sizeBytes: 410_000,
            total: 980,
            passed: 770,
            failed: 190,
            warning: 20,
            status: SubmissionStatus.Failed);

        var attendance = NewCollection(lea.Id, "attendance-kipp", "Attendance", new(2026, 9, 1), isActive: true);
        AddSubmission(
            attendance,
            "KIPP_Attendance_Fall2026_v1.xlsx",
            new DateTimeOffset(2026, 9, 1, 8, 40, 0, TimeSpan.Zero),
            sizeBytes: 860_000,
            total: 2100,
            passed: 2100,
            failed: 0,
            warning: 0,
            status: SubmissionStatus.Passed);

        lea.Collections.AddRange(
        [
            enrollment,
            discipline,
            attendance,
            NewCollection(lea.Id, "staff-kipp", "Staff", new(2025, 6, 30), isActive: false),
            NewCollection(lea.Id, "assessment-kipp", "Assessment", new(2025, 5, 15), isActive: false),
            NewCollection(lea.Id, "graduation-kipp", "Graduation", new(2025, 4, 1), isActive: false),
            NewCollection(lea.Id, "special-ed-kipp", "Special Education", new(2024, 9, 20), isActive: false),
            NewCollection(lea.Id, "title-i-kipp", "Title I", new(2024, 8, 1), isActive: false),
        ]);

        return lea;
    }

    private static Lea BuildFriendshipPcs()
    {
        var lea = new Lea
        {
            Id = "friendship-pcs",
            Name = "Friendship PCS",
            OrgLabel = "Friendship PCS",
            DisplayName = "Friendship Public Charter School",
        };

        var enrollment = NewCollection(lea.Id, "enrollment-friendship", "Enrollment", new(2026, 9, 10), isActive: true);
        AddSubmission(
            enrollment,
            "Friendship_Enrollment_Fall2026_v1.xlsx",
            new DateTimeOffset(2026, 9, 10, 11, 20, 0, TimeSpan.Zero),
            sizeBytes: 520_000,
            total: 1200,
            passed: 1190,
            failed: 0,
            warning: 10,
            status: SubmissionStatus.Passed);

        var course = NewCollection(lea.Id, "course-friendship", "Course", new(2026, 8, 22), isActive: true);
        AddSubmission(
            course,
            "Friendship_Course_Fall2026_v1.xlsx",
            new DateTimeOffset(2026, 8, 22, 15, 0, 0, TimeSpan.Zero),
            sizeBytes: 280_000,
            total: 640,
            passed: 640,
            failed: 0,
            warning: 0,
            status: SubmissionStatus.Passed);

        lea.Collections.AddRange(
        [
            enrollment,
            course,
            NewCollection(lea.Id, "staff-friendship", "Staff", new(2025, 6, 30), isActive: false),
            NewCollection(lea.Id, "assessment-friendship", "Assessment", new(2025, 5, 15), isActive: false),
            NewCollection(lea.Id, "graduation-friendship", "Graduation", new(2025, 4, 1), isActive: false),
            NewCollection(lea.Id, "attendance-friendship", "Attendance", new(2025, 3, 10), isActive: false),
            NewCollection(lea.Id, "special-ed-friendship", "Special Education", new(2024, 9, 20), isActive: false),
            NewCollection(lea.Id, "title-i-friendship", "Title I", new(2024, 8, 1), isActive: false),
        ]);

        return lea;
    }

    private static Lea BuildUdcCc()
    {
        var lea = new Lea
        {
            Id = "udc-cc",
            Name = "UDC-CC",
            OrgLabel = "UDC-CC",
            DisplayName = "University of DC Community College",
        };

        var enrollment = NewCollection(lea.Id, "enrollment-udc", "Enrollment", new(2026, 9, 5), isActive: true);
        AddSubmission(
            enrollment,
            "UDC_Enrollment_Fall2026_v1.xlsx",
            new DateTimeOffset(2026, 9, 5, 16, 30, 0, TimeSpan.Zero),
            sizeBytes: 95_000,
            total: 210,
            passed: 210,
            failed: 0,
            warning: 0,
            status: SubmissionStatus.Passed);

        lea.Collections.AddRange(
        [
            enrollment,
            NewCollection(lea.Id, "staff-udc", "Staff", new(2025, 6, 30), isActive: false),
            NewCollection(lea.Id, "assessment-udc", "Assessment", new(2025, 5, 15), isActive: false),
            NewCollection(lea.Id, "graduation-udc", "Graduation", new(2025, 4, 1), isActive: false),
        ]);

        return lea;
    }

    private static CollectionDefinition NewCollection(string leaId, string id, string name, DateOnly dueDate, bool isActive) =>
        new()
        {
            Id = id,
            LeaId = leaId,
            Name = name,
            DueDate = dueDate,
            IsActive = isActive,
            SchoolYear = SchoolYear,
        };

    private static void AddSubmission(
        CollectionDefinition collection,
        string fileName,
        DateTimeOffset uploadedAt,
        long sizeBytes,
        int total,
        int? passed,
        int? failed,
        int? warning,
        SubmissionStatus status)
    {
        collection.Submissions.Add(new Submission
        {
            Id = Guid.CreateVersion7(),
            CollectionId = collection.Id,
            LeaId = collection.LeaId,
            FileName = fileName,
            BlobPath = $"submissions/{collection.LeaId}/{collection.Id}/{fileName}",
            SizeBytes = sizeBytes,
            UploadedBy = "shiva.chikkula@dc.gov",
            UploadedAt = uploadedAt,
            ProcessedAt = status is SubmissionStatus.Processing or SubmissionStatus.Uploaded ? null : uploadedAt,
            Status = status,
            TotalRecords = total,
            PassedRecords = passed,
            FailedRecords = failed,
            WarningRecords = warning,
        });
    }

    private static IEnumerable<NotificationEntity> BuildNotifications()
    {
        yield return new NotificationEntity
        {
            Id = Guid.CreateVersion7(),
            Severity = NotificationSeverity.Info,
            Title = "CLSD End of Year (EOY) window is open",
            Message =
                "The CLSD End of Year (EOY) data collection window is open Jul 28 - Aug 18, 2026. " +
                "Please upload data as soon as possible after fixing all errors. Questions: Clara.Smith@dc.gov.",
            CreatedAt = new DateTimeOffset(2026, 7, 28, 0, 0, 0, TimeSpan.Zero),
            IsRead = false,
        };

        yield return new NotificationEntity
        {
            Id = Guid.CreateVersion7(),
            Severity = NotificationSeverity.Error,
            Title = "CLSD-LEA upload failed",
            Message = "The last submission for CLSD-LEA had 9,134 validation errors. Review and resubmit before the due date.",
            CreatedAt = new DateTimeOffset(2026, 8, 20, 14, 47, 0, TimeSpan.Zero),
            IsRead = false,
            LeaId = "dcps",
            CollectionId = "clsd-lea",
        };

        yield return new NotificationEntity
        {
            Id = Guid.CreateVersion7(),
            Severity = NotificationSeverity.Warning,
            Title = "Course collection due soon",
            Message = "The Course collection is due Aug 19, 2026 - 3 days remaining.",
            CreatedAt = new DateTimeOffset(2026, 8, 16, 0, 0, 0, TimeSpan.Zero),
            IsRead = false,
            LeaId = "dcps",
            CollectionId = "course",
        };

        yield return new NotificationEntity
        {
            Id = Guid.CreateVersion7(),
            Severity = NotificationSeverity.Info,
            Title = "Discipline collection passed validation",
            Message = "DCPS_Enrollment_Fall2026_v3.xlsx passed with 4,822 of 4,908 records clean.",
            CreatedAt = new DateTimeOffset(2026, 8, 10, 0, 0, 0, TimeSpan.Zero),
            IsRead = true,
            LeaId = "dcps",
            CollectionId = "discipline",
        };
    }
}
