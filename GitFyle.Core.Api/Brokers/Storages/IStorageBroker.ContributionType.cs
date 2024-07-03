using GitFyle.Core.Api.Models.Foundations.ContributionTypes;
using System.Threading.Tasks;

namespace GitFyle.Core.Api.Brokers.Storages
{
    internal partial interface IStorageBroker
    {
        ValueTask<ContributionType> InsertContributionTypeAsync(ContributionType contributionType);
    }
}
