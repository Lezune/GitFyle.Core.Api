using GitFyle.Core.Api.Models.Foundations.Repositories;
using System.Threading.Tasks;

namespace GitFyle.Core.Api.Services.Foundations.Repositories
{
    public interface IRepositoryService
    {
        ValueTask<Repository> AddAsync(Repository repository);
    }
}
