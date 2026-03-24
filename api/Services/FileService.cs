using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using System.IO;

namespace api.Services;

public class FileService(ILogger<IFileService> logger, IMinioClient minioClient, IOptions<S3Options> s3Config) : IFileService
{
    public async Task Save(PutObjectModel model, CancellationToken cancellationToken)
    {
        var putObjectArgs = new PutObjectArgs()
                    .WithBucket(s3Config.Value.BucketName)
                    .WithObject(model.ObjectName)
                    .WithStreamData(model.Data)
                    .WithContentType(model.ContentType)
                    .WithObjectSize(model.Data.Length);

        var putResponse = await minioClient.PutObjectAsync(putObjectArgs);

        logger.LogInformation("Successfully uploaded " + model.ObjectName);
    }

    /*
    public async Task<List<Tuple<string, string>>> ListCardObjects(IMinioClient client, string bucketName, Guid cardGuid, CancellationToken ct)
    {
        List<Tuple<string, string>> allObj = new List<Tuple<string, string>>();
        ListObjectsArgs args = new ListObjectsArgs().WithBucket(bucketName).WithVersions(true).WithRecursive(true);
        var obs = await client.ListObjectsEnumAsync(args, ct).Where(d => d.Key.StartsWith(cardGuid.ToString())).ToArrayAsync(ct);
        foreach (var o in obs)
        {
            allObj.Add(new Tuple<string, string>(o.Key, o.VersionId));
        }
        return allObj;
    }

    public async Task<List<Tuple<string, string>>> ListOutdatedObjects(IMinioClient client, string bucketName, DateTime? dateTo)
    {
        List<Tuple<string, string>> allObj = new List<Tuple<string, string>>();
        ListObjectsArgs args = new ListObjectsArgs().WithBucket(bucketName).WithVersions(true).WithRecursive(true);
        DateTime compareDate = dateTo == null ? DateTime.UtcNow.AddMonths(-_s3Config.StoreForMonths) : dateTo.Value;
        var obs = await client.ListObjectsAsync(args).Where(d => d.LastModifiedDateTime < compareDate).ToArray();
        foreach (var o in obs)
        {
            allObj.Add(new Tuple<string, string>(o.Key, o.VersionId));
        }
        return allObj;
    }


    /// <inheritdoc cref="IFileService"/>
    public async Task<List<string>> GetBase64String(Guid cardGuid, string[] inspectionStepKey, CancellationToken token)
    {
        try
        {
            List<string> bases = [];
            string[] urls = await _photoRepository.GetPhotoUrls(cardGuid, inspectionStepKey, token);
            foreach (var url in urls)
            {
                if (!String.IsNullOrEmpty(url))
                {
                    string[] v = url.Split('/');
                    string bucketName = v[3];
                    string fileName = v[4] + '/' + v[5] + '/' + v[6].Split('?')[0];
                    if (await IsObjectExist(fileName))
                    {
                        await _minioClient.GetObjectAsync(new GetObjectArgs().WithBucket(bucketName).WithObject(fileName).WithCallbackStream((stream) =>
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                stream.CopyTo(ms);
                                bases.Add("data:image/jpeg;base64," + Convert.ToBase64String(ms.ToArray()));
                            }
                        }));
                    }
                }
            }
            return bases;
        }
        catch (Exception ex)
        {
            _logger.LogError("Исключение при получении изображений base64 для карточки '{VehicleCardGuid}' из S3: {msg}.", cardGuid, ex.Message);

            throw;
        }

    }

    private async Task<bool> IsObjectExist(String name)
    {
        //minioClient генерирует исключение при отсутствии объекта
        try
        {
            var res = await _minioClient.StatObjectAsync(new StatObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(name));
            if (res.ETag != null)
            {
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            //объект не обнаружен
            return false;
        }
    }
    */
}

public record PutObjectModel(Stream Data, string ObjectName, string ContentType);