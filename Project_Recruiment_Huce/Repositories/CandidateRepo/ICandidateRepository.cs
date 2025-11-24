using System;
using System.Collections.Generic;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories.CandidateRepo
{
    /// <summary>
    /// Interface cho repository quản lý candidate data access operations
    /// </summary>
    public interface ICandidateRepository
    {
        /// <summary>
        /// Lấy candidate theo AccountID
        /// </summary>
        Candidate GetCandidateByAccountId(int accountId);

        /// <summary>
        /// Lấy candidate theo CandidateID
        /// </summary>
        Candidate GetCandidateById(int candidateId);

        /// <summary>
        /// Tạo candidate mới
        /// </summary>
        void CreateCandidate(Candidate candidate);

        /// <summary>
        /// Cập nhật thông tin candidate
        /// </summary>
        void UpdateCandidate(Candidate candidate);

        /// <summary>
        /// Lấy ProfilePhoto theo PhotoID
        /// </summary>
        ProfilePhoto GetProfilePhoto(int photoId);

        /// <summary>
        /// Lưu ProfilePhoto và trả về PhotoID
        /// </summary>
        int SaveProfilePhoto(ProfilePhoto photo);

        /// <summary>
        /// Cập nhật PhotoID cho candidate và account
        /// </summary>
        void UpdatePhotoId(int candidateId, int accountId, int photoId);

        /// <summary>
        /// Lấy danh sách ResumeFiles của candidate
        /// </summary>
        List<ResumeFile> GetResumeFiles(int candidateId);

        /// <summary>
        /// Lưu ResumeFile mới
        /// </summary>
        int SaveResumeFile(ResumeFile resumeFile);

        /// <summary>
        /// Lấy ResumeFile theo ID
        /// </summary>
        ResumeFile GetResumeFile(int resumeFileId, int candidateId);

        /// <summary>
        /// Lấy danh sách applications của candidate
        /// </summary>
        List<Application> GetApplications(int candidateId);

        /// <summary>
        /// Lấy JobPost theo ID
        /// </summary>
        JobPost GetJobPost(int jobPostId);

        /// <summary>
        /// Kiểm tra xem candidate đã apply cho job chưa
        /// </summary>
        Application GetExistingApplication(int candidateId, int jobPostId);

        /// <summary>
        /// Tạo application mới
        /// </summary>
        void CreateApplication(Application application);

        /// <summary>
        /// Kiểm tra phone uniqueness (trừ accountId hiện tại)
        /// </summary>
        bool IsPhoneUnique(string phone, int accountId);

        /// <summary>
        /// Lưu tất cả thay đổi vào database
        /// </summary>
        void SaveChanges();
    }
}
