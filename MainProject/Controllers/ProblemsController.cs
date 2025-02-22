﻿using AutoMapper;
using CodeforcesTool.Entity;
using MainProject.Models.Helpers;
using MainProject.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MainProject.Controllers
{
    [Route("api/problems")]
    public class ProblemsController : Controller
    {
        private readonly IRepository repo;
        private readonly IRecommendationRepository recRepo;
        private readonly IUrlHelper urlHelper;

        public ProblemsController(IRepository _repo,IUrlHelper _url, IRecommendationRepository rec)
        {
            repo = _repo;
            urlHelper = _url;
            recRepo = rec;
        }

        [HttpGet(Name ="GetProblems")]
        public IActionResult GetProblems(HomePageParameters homePageParameters) {

            var problemsFromRepo = repo.GetProblems(homePageParameters);

            var previousPageLink = problemsFromRepo.HasPrevious
                                    ? CreateProblemsResourceUri(homePageParameters,ResourceUriType.PreviousPage)
                                    : null;

            var nextPageLink = problemsFromRepo.HasNext
                                    ? CreateProblemsResourceUri(homePageParameters, ResourceUriType.NextPage)
                                    : null;

            var paginationMetaData = new
            {
                totalCount = problemsFromRepo.TotalCount,
                pageSize = problemsFromRepo.PageSize,
                currentPage = problemsFromRepo.CurrentPage,
                totalPages = problemsFromRepo.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };

            Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetaData));

            var problems = repo.toDto(problemsFromRepo);
            
            if( ! string.IsNullOrEmpty ( homePageParameters.UserHandle))
            {
                var user = repo.GetUser(homePageParameters.UserHandle);
                if( user != null)
                {
                    foreach(var problem in problems)
                    {
                        problem.Solved = repo.IsSolved(user.Id, problem.Id);
                    }
                }
            }

            return Ok(new { problems, pagedList= paginationMetaData.totalCount });
        }

        private string CreateProblemsResourceUri(HomePageParameters homePageParameters,ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetProblems", new
                    {
                        solved = homePageParameters.Solved,
                        userName = homePageParameters.UserHandle,
                        tagName = homePageParameters.TagName,
                        pageNumber = homePageParameters.PageNumber-1,
                        pageSize = homePageParameters.PageSize
                    });

                case ResourceUriType.NextPage:
                    return Url.Link("GetProblems", new
                    {
                        solved = homePageParameters.Solved,
                        userName = homePageParameters.UserHandle,
                        tagName = homePageParameters.TagName,
                        pageNumber = homePageParameters.PageNumber + 1,
                        pageSize = homePageParameters.PageSize
                    });
                default:
                    return Url.Link("GetProblems", new
                    {
                        solved = homePageParameters.Solved,
                        userName = homePageParameters.UserHandle,
                        tagName = homePageParameters.TagName,
                        pageNumber = homePageParameters.PageNumber ,
                        pageSize = homePageParameters.PageSize
                    });

            }
        }

        [HttpGet("rec/{userId}")]
        public IActionResult GetUserCorr(Guid userId)
        {
            var sugs = recRepo.GetUserFriendSug(userId);
            return Ok(sugs);
        }


        [HttpGet("rec/problems/{userId}")]
        public IActionResult GetUser(Guid userId)
        {

            var problems =  recRepo.GetUserProblemSug(userId);

            return Ok(problems);

        }
    }
}
