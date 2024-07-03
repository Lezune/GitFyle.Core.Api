using GitFyle.Core.Api.Models.Foundations.ContributionTypes;
using System.Linq;
using System.Threading.Tasks;

namespace GitFyle.Core.Api.Brokers.Storages
{
    internal partial interface IStorageBroker
    {
        ValueTask<ContributionType> DeleteContributionTypeAsync(ContributionType contributionType);
    }
}
