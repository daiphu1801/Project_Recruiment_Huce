using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories
{
    /// <summary>
    /// Repository interface for JobPost entity
    /// Defines contract for data access operations
    /// </summary>
    public interface IJobRepository
    {
        // Basic CRUD operations
        JobPost GetById(int id);
        IEnumerable<JobPost> GetAll();
        void Add(JobPost job);
        void Update(JobPost job);
        void Delete(int id);
        void SaveChanges();

        // Query operations
        IEnumerable<JobPost> Find(Expression<Func<JobPost, bool>> predicate);
        JobPost FirstOrDefault(Expression<Func<JobPost, bool>> predicate);
        bool Any(Expression<Func<JobPost, bool>> predicate);
        int Count(Expression<Func<JobPost, bool>> predicate);

        // Specific business queries
        IEnumerable<JobPost> GetPublishedJobs();
        IEnumerable<JobPost> GetJobsByRecruiter(int recruiterId);
        IEnumerable<JobPost> GetJobsByCompany(int companyId);
        IEnumerable<JobPost> GetRelatedJobs(int jobId, int companyId, string location, int take = 5);
        IEnumerable<JobPost> SearchJobs(string keyword, string location, string employmentType);
        
        // With eager loading
        JobPost GetByIdWithDetails(int id);
        IEnumerable<JobPost> GetAllWithDetails();
    }
}

