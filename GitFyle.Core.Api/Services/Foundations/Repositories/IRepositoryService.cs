using GitFyle.Core.Api.Models.Foundations.Repositories;
using System.Threading.Tasks;

namespace GitFyle.Core.Api.Services.Foundations.Repositories
{
    internal interface IRepositoryService
    {
        ValueTask<Repository> AddAsync(Repository repository);
    }
}
