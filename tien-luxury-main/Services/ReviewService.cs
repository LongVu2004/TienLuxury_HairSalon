using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using TienLuxury.Helpers;
using TienLuxury.Models;

namespace TienLuxury.Services
{
    public class ReviewService : IReviewService
    {
        private readonly DBContext _context;

        public ReviewService(DBContext context)
        {
            _context = context;
        }

        public async Task<List<ProductReview>> GetAllReviewsAsync()
        {
            var reviews = await _context.Set<ProductReview>().ToListAsync();

            return reviews.OrderBy(r => r.IsApproved.GetValueOrDefault()) // False (hoặc null) lên trước
                  .ThenByDescending(r => r.CreatedAt)
                  .ToList();
        }

        public async Task ApproveReviewAsync(string id)
        {
            if (ObjectId.TryParse(id, out ObjectId objId))
            {
                var review = await _context.Set<ProductReview>().FindAsync(objId);
                if (review != null)
                {
                    review.IsApproved = true;
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task ApproveReviewWithCensorshipAsync(string id)
        {
            if (ObjectId.TryParse(id, out ObjectId objId))
            {
                var review = await _context.Set<ProductReview>().FindAsync(objId);
                if (review != null)
                {
                    // 1. Gọi Helper để che từ xấu
                    review.Comment = ProfanityHelper.CensorText(review.Comment);

                    // 2. Duyệt như bình thường
                    review.IsApproved = true;

                    // 3. Lưu lại
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteReviewAsync(string id)
        {
            if (ObjectId.TryParse(id, out ObjectId objId))
            {
                var review = await _context.Set<ProductReview>().FindAsync(objId);
                if (review != null)
                {
                    _context.Remove(review);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}