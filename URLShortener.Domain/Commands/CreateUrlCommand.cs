using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.EntityFrameworkCore;

using UrlShortener.Contracts.Database;
using UrlShortener.Domain.Database;

namespace UrlShortener.Domain.Commands;

public class CreateUrlCommand : IRequest<CreateUrlCommandResult>
{
    public string OriginUrl { get; init; }

}

public class CreateUrlCommandResult
{
    public int Id { get; set; }
    public string ShortenedUrl { get; init; }
}

internal class CreateUrlCommandHandler : IRequestHandler<CreateUrlCommand, CreateUrlCommandResult>
{

    private readonly UrlDbContext _dbContext;

    public CreateUrlCommandHandler(UrlDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateUrlCommandResult> Handle(CreateUrlCommand request, CancellationToken cancellationToken = default)
    {

        if (!ValidationCheck(request.OriginUrl))
        {
            throw new Exception("Validation error, please use link format");
        }
        var url = new Url
        {
            OriginUrl = request.OriginUrl,
            ShortenedUrl = GenerateShortUrl(5)
        };

        try
        {
            await _dbContext.AddAsync(url, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            var urlCheck = await _dbContext.Urls.FirstOrDefaultAsync(u => u.OriginUrl == request.OriginUrl, cancellationToken);
            return new CreateUrlCommandResult
            {
                ShortenedUrl = urlCheck.ShortenedUrl,
                Id = urlCheck.Id
            };
        }
        return new CreateUrlCommandResult
        {
            ShortenedUrl = url.ShortenedUrl,
            Id = url.Id
        };
    }

    private string GenerateShortUrl(int length)
    {
        Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(u => u[random.Next(u.Length)]).ToArray());
    }

    private bool ValidationCheck(String uriName)
    {
        if (String.IsNullOrWhiteSpace(uriName)) return false;
        Uri uriResult;
        bool result = Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
         && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        return result;
    }

}