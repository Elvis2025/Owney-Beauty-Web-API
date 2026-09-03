using System;
using System.Threading.Tasks;
using Ieh.OwneyBeauty.Shared;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Ieh.OwneyBeauty.Authors;

public interface IAuthorAppService :
    ICrudAppService<
        AuthorDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateAuthorDto>
{
    Task<IRemoteStreamContent> GetListAsExcelFileAsync(AuthorExcelDownloadDto input);

    Task<DownloadTokenResultDto> GetDownloadTokenAsync();
}
