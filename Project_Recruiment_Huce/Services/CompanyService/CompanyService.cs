using System;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Companies;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Repositories;

namespace Project_Recruiment_Huce.Services
{
    /// <summary>
    /// Service xử lý business logic cho Company (tạo, cập nhật, lấy dữ liệu cho view)
    /// Giúp tách controller khỏi truy vấn trực tiếp vào DataContext.
    /// </summary>
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _repo;

        public CompanyService(ICompanyRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public CompanyManageViewModel GetCompanyManageViewModel(int accountId)
        {
            var recruiter = _repo.GetRecruiterByAccountId(accountId);
            if (recruiter == null) return null;

            Company company = null;
            if (recruiter.CompanyID.HasValue)
            {
                company = _repo.GetCompanyById(recruiter.CompanyID.Value);
            }

            var vm = new CompanyManageViewModel
            {
                CompanyID = company?.CompanyID,
                CompanyName = company?.CompanyName,
                TaxCode = company?.TaxCode,
                Industry = company?.Industry,
                Address = company?.Address,
                Phone = company?.Phone,
                Fax = company?.Fax,
                CompanyEmail = company?.CompanyEmail,
                Website = company?.Website,
                Description = company?.Description,
                PhotoID = company?.PhotoID
            };

            if (company?.PhotoID.HasValue == true)
            {
                var photo = _repo.GetProfilePhotoById(company.PhotoID.Value);
                if (photo != null) vm.LogoUrl = photo.FilePath;
            }

            return vm;
        }

        public ValidationResult SaveOrUpdateCompany(CompanyManageViewModel viewModel, int accountId, int? newPhotoId = null)
        {
            var result = new ValidationResult();

            // Basic validation (controller already validates modelstate/phone/email etc.)
            if (viewModel == null)
            {
                result.IsValid = false;
                result.Errors[""] = "Dữ liệu form không hợp lệ.";
                return result;
            }

            var recruiter = _repo.GetRecruiterByAccountId(accountId);
            if (recruiter == null)
            {
                result.IsValid = false;
                result.Errors["Recruiter"] = "Vui lòng tạo hồ sơ nhà tuyển dụng trước.";
                return result;
            }

            Company company = null;
            if (recruiter.CompanyID.HasValue)
            {
                company = _repo.GetCompanyById(recruiter.CompanyID.Value);
            }

            if (newPhotoId.HasValue)
            {
                if (company == null)
                {
                    company = new Company
                    {
                        CompanyName = viewModel.CompanyName,
                        TaxCode = viewModel.TaxCode,
                        Industry = viewModel.Industry,
                        Address = viewModel.Address,
                        Phone = viewModel.Phone,
                        Fax = viewModel.Fax,
                        CompanyEmail = viewModel.CompanyEmail,
                        Website = viewModel.Website,
                        Description = !string.IsNullOrWhiteSpace(viewModel.Description) ? HtmlSanitizerHelper.Sanitize(viewModel.Description) : null,
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1,
                        PhotoID = newPhotoId.Value
                    };

                    _repo.InsertCompany(company);
                    _repo.SaveChanges();

                    // Link recruiter
                    recruiter.CompanyID = company.CompanyID;
                    _repo.SaveChanges();
                }
                else
                {
                    var oldPhotoId = company.PhotoID;

                    // Update fields
                    company.CompanyName = viewModel.CompanyName;
                    company.TaxCode = viewModel.TaxCode;
                    company.Industry = viewModel.Industry;
                    company.Address = viewModel.Address;
                    company.Phone = viewModel.Phone;
                    company.Fax = viewModel.Fax;
                    company.CompanyEmail = viewModel.CompanyEmail;
                    company.Website = viewModel.Website;
                    company.Description = !string.IsNullOrWhiteSpace(viewModel.Description) ? HtmlSanitizerHelper.Sanitize(viewModel.Description) : null;
                    company.PhotoID = newPhotoId.Value;

                    try
                    {
                        _repo.SaveChanges();

                        // If there was an old photo, request caller to delete physical file
                        if (oldPhotoId.HasValue)
                        {
                            var oldPhoto = _repo.GetProfilePhotoById(oldPhotoId.Value);
                            if (oldPhoto != null)
                            {
                                _repo.DeleteProfilePhoto(oldPhoto);
                                _repo.SaveChanges();
                                result.Data["OldPhotoId"] = oldPhotoId.Value;
                            }
                        }
                    }
                    catch (System.Data.Linq.ChangeConflictException)
                    {
                        // Try to refresh and retry
                        // Note: caller may provide a writable context; we assume repo's context is writable
                        throw;
                    }
                }
            }
            else
            {
                // No new photo; create or update other fields
                if (company == null)
                {
                    company = new Company
                    {
                        CompanyName = viewModel.CompanyName,
                        TaxCode = viewModel.TaxCode,
                        Industry = viewModel.Industry,
                        Address = viewModel.Address,
                        Phone = viewModel.Phone,
                        Fax = viewModel.Fax,
                        CompanyEmail = viewModel.CompanyEmail,
                        Website = viewModel.Website,
                        Description = !string.IsNullOrWhiteSpace(viewModel.Description) ? HtmlSanitizerHelper.Sanitize(viewModel.Description) : null,
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };
                    _repo.InsertCompany(company);
                    _repo.SaveChanges();

                    recruiter.CompanyID = company.CompanyID;
                    _repo.SaveChanges();
                }
                else
                {
                    company.CompanyName = viewModel.CompanyName;
                    company.TaxCode = viewModel.TaxCode;
                    company.Industry = viewModel.Industry;
                    company.Address = viewModel.Address;
                    company.Phone = viewModel.Phone;
                    company.Fax = viewModel.Fax;
                    company.CompanyEmail = viewModel.CompanyEmail;
                    company.Website = viewModel.Website;
                    company.Description = !string.IsNullOrWhiteSpace(viewModel.Description) ? HtmlSanitizerHelper.Sanitize(viewModel.Description) : null;

                    _repo.SaveChanges();
                }
            }

            result.IsValid = true;
            result.Data["CompanyId"] = company?.CompanyID;
            return result;
        }
    }
}
