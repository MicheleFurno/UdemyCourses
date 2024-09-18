using System;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository(DataContext context, IMapper mapper) : ILikesRepository
{
    public void AddLike(UserLike like)
    {
        context.Likes.Add(like);
    }

    public void DeleteLike(UserLike like)
    {
       context.Likes.Remove(like);
    }

    public async Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId)
    {
        return await context.Likes.Where(l => l.SourceUserId == currentUserId).Select(l => l.TargetUserId).ToListAsync();
    }

    public async Task<UserLike?> GetUserLike(int sourceUserId, int targetUserId)
    {
        return await context.Likes.FindAsync(sourceUserId, targetUserId);
    }

    public async Task<PagedList<MemberDTO>> GetUserLikes(LikesParams likesParams)
    {
        var likes = context.Likes.AsQueryable();

        IQueryable<MemberDTO> query;

        switch(likesParams.Predicate) {
            case "likes":
                query = likes.Where(l => l.SourceUserId == likesParams.UserId)
                                  .Select(l => l.TargetUser)
                                  .ProjectTo<MemberDTO>(mapper.ConfigurationProvider);
                break;
            case "likedBy":
                query = likes.Where(l => l.TargetUserId == likesParams.UserId)
                                  .Select(l => l.SourceUser)
                                  .ProjectTo<MemberDTO>(mapper.ConfigurationProvider);
                break;
            default:
                var likeIds = await GetCurrentUserLikeIds(likesParams.UserId);

                query = likes.Where(l=> l.TargetUserId == likesParams.UserId && likeIds.Contains(l.SourceUserId))
                                  .Select(l => l.SourceUser)
                                  .ProjectTo<MemberDTO>(mapper.ConfigurationProvider);
                break;
        }

        return await PagedList<MemberDTO>.CreateAsync(query, likesParams.pageNumber, likesParams.PageSize);
    }

    public async Task<bool> SaveChanges()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
