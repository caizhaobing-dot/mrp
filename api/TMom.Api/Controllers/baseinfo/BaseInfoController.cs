using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using TMom.Domain.Model.Entity;
using TMom.Application.Service.IService;
using static TMom.Domain.Model.GlobalVars;
using TMom.Application;
using TMom.Domain.Model;
using TMom.Application.Dto;

namespace TMom.Api.Controllers
{
    /// <summary>
    /// 测试菜单
    /// </summary>
	[Route("api/[controller]/[action]")]
	[ApiController]
     public class BaseInfoController: BaseApiController<BaseInfo, int>
    {
        private readonly IBaseInfoService _baseInfoService;
        private readonly IMapper _mapper;

        public BaseInfoController(IBaseInfoService baseInfoService, IMapper mapper)
        {
            _baseInfoService = baseInfoService;
            _mapper = mapper;
        }

        #region 模板生成 CRUD

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="pageIndex">页标, 默认1</param>
        /// <param name="pageSize">页数, 默认10</param>
        /// <param name="field">排序字段</param>
        /// <param name="order">排序类型: ascend|descend</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<PageModel<BaseInfo>>> GetWithPage(int pageIndex = 1, int pageSize = 10, string field = "", string order = "")
        {
            PageModel<BaseInfo> data = await _baseInfoService.GetWithPage(DynamicFilterExpress(), pageIndex, pageSize, FormatOrderField(field, order));
            return SuccessPage(data);
        }

        /// <summary>
        /// 根据主键Id获取单条数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<MessageModel<BaseInfo>> Get(int id)
        {
            BaseInfo entity = await _baseInfoService.GetById(id);
            return Success(entity);
        }

        /// <summary>
        /// 添加一条数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> Add([FromBody] BaseInfo model)
        {
            var id = await _baseInfoService.AddData(model);
            return id > 0 ? Success(id.ObjToString(), "添加成功!") : Failed();
        }

        /// <summary>
        /// 修改全部数据(默认根据主键更新)
        /// </summary>
        /// <param name="model"></param>
        /// <returns>修改的主键Id</returns>
        [HttpPut]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> Update([FromBody] BaseInfo model)
        {
            bool res = await _baseInfoService.UpdateData(model);
            return res ? Success("更新成功!") : Failed();
        }

        /// <summary>
        /// 根据主键Id删除数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> Delete(string ids)
        {
            bool res = await _baseInfoService.DeleteData(ids);
            return res ? Success("删除成功!") : Failed();
        }

        #endregion 模板生成 CRUD
        [HttpPost]
        public async Task SendMail()
        {
            await _baseInfoService.SendMail();
        }
    }
}