namespace RefactoringAssessment;

public class UserRequest
{
    public string UserName { get; set; }
    public string Data { get; set; }
    public UserType UserType { get; set; }
}

public enum UserType
{
    Basic,
    Premium,
    Admin
}