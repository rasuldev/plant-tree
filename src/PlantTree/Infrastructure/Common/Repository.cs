using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PlantTree.Data;
using PlantTree.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PlantTree.Infrastructure.Common
{
    /// <summary>
    /// Contains some common queries to context. Supports caching.
    /// </summary>
    public class Repository
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<Repository> _logger;
        private readonly int[] _cachePageList = { 1, 2, 3 };
        private readonly int[] _cachePageSizeList = { 10, 15, 20, 25, 30 };

        public Repository(AppDbContext context, IMemoryCache cache, ILogger<Repository> logger)
        {
            _cache = cache;
            _logger = logger;
            _context = context;
            _context.SavingChanges += ContextOnSavingChanges;
        }

        public bool UseCache { get; set; } = false;

        private void ContextOnSavingChanges(object sender, EventArgs eventArgs)
        {
            if (!UseCache) return;
            ProcessProjectsCache();
        }

        #region Projects

        private bool AllowCachingProjects(int page, int pagesize)
        {
            // TODO: cache was temporary disabled. We need to clarify caching policy
            return false;
            //return _cachePageList.Contains(page) && _cachePageSizeList.Contains(pagesize);
        }

        private static string GetProjectsCacheKey(ProjectStatus status, int page, int pagesize)
        {
            return $"projects-status-{status}-page{page}-size{pagesize}";
        }

        public async Task<List<Project>> GetProjects(ProjectStatus status, int page, int pagesize)
        {
            List<Project> projects;
            var cacheKey = GetProjectsCacheKey(status, page, pagesize);
            if (_cache.TryGetValue(cacheKey, out projects)) return projects;

            projects = await _context.Projects.Where(p => p.Status == status)
                .Include(p => p.MainImage).Include(p => p.OtherImages).Include(p => p.ProjectUsers)
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToListAsync();

            // Place in cache only first several pages 
            // We cache only limited amount of pagesizes to protect from cache-overflow attack
            if (UseCache && AllowCachingProjects(page, pagesize))
            {
                _cache.Set(cacheKey, projects);
            }

            return projects;
        }

        /// <summary>
        /// Removes cache entry for projects of specified page and pagesize
        /// </summary>
        /// <param name="status"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        public void InvalidateProjectsCache(ProjectStatus status, int page, int pagesize)
        {
            var cacheKey = GetProjectsCacheKey(status, page, pagesize);
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
                    foreach (ProjectStatus status in Enum.GetValues(typeof(ProjectStatus)))
                    {
                        InvalidateProjectsCache(status, page, pagesize);
                    }
                }
            }
        }

        protected void ProcessProjectsCache()
        {
            if (_context.ChangeTracker.Entries<Project>().Any(p => p.State != EntityState.Unchanged))
            {
                InvalidateAllProjectsCache();
            }
        }
        #endregion
    }
}