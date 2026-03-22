using System.Collections.Generic;
using System.Threading.Tasks;
using TienLuxury.Models;

namespace TienLuxury.Services
{
    public interface IReviewService
    {
        Task<List<ProductReview>> GetAllReviewsAsync();

        Task ApproveReviewAsync(string id);

        Task DeleteReviewAsync(string id);
        Task ApproveReviewWithCensorshipAsync(string id);
    }
}