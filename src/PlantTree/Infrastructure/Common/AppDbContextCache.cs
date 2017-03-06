using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PlantTree.Data;
using PlantTree.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace PlantTree.Infrastructure.Common
{
    public class AppDbContextCache
    {
        public AppDbContext Context { get; set; }
        private readonly IMemoryCache _cache;
        private readonly ILogger<AppDbContextCache> _logger;
        private readonly int[] _cachePageList = { 1, 2, 3 };
        private readonly int[] _cachePageSizeList = { 10, 15, 20, 25, 30 };

        public AppDbContextCache(IMemoryCache cache, ILogger<AppDbContextCache> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        #region Projects

        private bool AllowCachingProjects(int page, int pagesize)
        {
            // TODO: cache was temporary disabled. We need to clarify caching policy
            return false;
            //return _cachePageList.Contains(page) && _cachePageSizeList.Contains(pagesize);
        }

        private static string GetProjectsCacheKey(int page, int pagesize)
        {
            return $"projects-page{page}-size{pagesize}";
        }

        public async Task<List<Project>> GetProjects(int page, int pagesize)
        {
            List<Project> projects;
            var cacheKey = GetProjectsCacheKey(page, pagesize);
            if (_cache.TryGetValue(cacheKey, out projects)) return projects;

            projects = await Context.Projects
                .Include(p => p.MainImage).Include(p => p.OtherImages).Include(p => p.ProjectUsers)
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToListAsync();

            // Place in cache only first several pages 
            // We cache only limited amount of pagesizes to protect from cache-overflow attack
            if (AllowCachingProjects(page, pagesize))
            {
                _cache.Set(cacheKey, projects);
            }

            return projects;
        }

        /// <summary>
        /// Removes cache entry for projects of specified page and pagesize
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        public void InvalidateProjectsCache(int page, int pagesize)
        {
            var cacheKey = GetProjectsCacheKey(page, pagesize);
            _cache.Remove(cacheKey);
        }

        /// <summary>
        /// Removes projects cache for all pages and pagesizes
        /// </summary>
        public void InvalidateAllProjectsCache()
        {
            _logger.LogInformation("Projects cache is invalidated");
            foreach (var page in _cachePageList)
            {
                foreach (var pagesize in _cachePageSizeList)
                {
                    InvalidateProjectsCache(page, pagesize);
                }
            }
        }

        #endregion
    }
}