namespace WatchStoreApi.Application.Common;

public sealed record ProductImage(Stream Content, string FileName, long Length);
