using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories
{
    /// <summary>
    /// Repository interface for Application entity
    /// Defines contract for application data access operations
    /// </summary>
    public interface IApplicationRepository
    {
        // Basic CRUD operations
        Application GetById(int id);
        IEnumerable<Application> GetAll();
        void Add(Application application);
        void Update(Application application);
        void Delete(int id);
        void SaveChanges();

        // Query operations
        IEnumerable<Application> Find(Expression<Func<Application, bool>> predicate);
        Application FirstOrDefault(Expression<Func<Application, bool>> predicate);
        bool Any(Expression<Func<Application, bool>> predicate);
        int Count(Expression<Func<Application, bool>> predicate);

        // Specific business queries
        IEnumerable<Application> GetApplicationsByCandidate(int candidateId);
        IEnumerable<Application> GetApplicationsByJob(int jobPostId);
        IEnumerable<Application> GetApplicationsByRecruiter(int recruiterId);
        IEnumerable<Application> GetApplicationsByStatus(string status);
        int GetPendingApplicationsCount(int jobPostId);
        
        // With eager loading
        Application GetByIdWithDetails(int id);
        IEnumerable<Application> GetAllWithDetails();
    }
}

