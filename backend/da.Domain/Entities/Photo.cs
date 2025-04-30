using System.Runtime.InteropServices;

namespace da.Domain.Entities;

public class Photo(
    string Schema,
    string Url165,
    string Url256,
    string UrlEnlarged,
    [Optional] bool IsMain
);