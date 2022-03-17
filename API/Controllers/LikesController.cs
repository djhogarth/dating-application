using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using API.DataTransferObjects;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
    private readonly IUserRepository _userRepository;
    private readonly ILikesRepository _likesRepository;

    public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
      _userRepository = userRepository;
      _likesRepository = likesRepository;
    }

    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username)
    {
      var sourceUserId = User.GetUserId();
      var likedUser = await _userRepository.GetUserByUsernameAsync(username);
      var sourceUser = await _likesRepository.GeUserWithLikes(sourceUserId);

      var userLike = await _likesRepository.GetAppUserLike(sourceUserId, likedUser.ID);
      if(likedUser == null) return NotFound();

      //prevent user from liking themselves
      if(sourceUser.UserName == username)
        return BadRequest("You cannot like yourself");

      if(userLike != null) return BadRequest("You already liked this user");

      userLike = new AppUserLike
      {
        SourceUserId = sourceUserId,
        LikedUserId = likedUser.ID
      };

      sourceUser.LikedUsers.Add(userLike);

      if(await _userRepository.SaveAllChangesAsync()) return Ok();

      return BadRequest("Failed to like the user");

    }

    [HttpGet]

    public async Task<ActionResult<IEnumerable<LikeDTO>>> GetUserLikes([FromQuery]LikesParameters likesParams){

      likesParams.UserId = User.GetUserId();
      var users = await _likesRepository.GetUserLikes(likesParams);
      Response.AddPaginationHeader(users.CurrentPage, users.PageSize,
       users.TotalCount, users.TotalPages);

      return Ok(users);
    }

    }
}
