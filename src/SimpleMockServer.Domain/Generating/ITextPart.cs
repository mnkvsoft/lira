namespace SimpleMockServer.Domain.Generating;

public interface ITextPart
{
    string? Get(RequestData request);
}

//public interface IGlobalTextPart : ITextPart
//{
//    string? Get();
//}
