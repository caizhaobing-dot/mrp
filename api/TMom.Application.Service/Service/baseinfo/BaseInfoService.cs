using TMom.Application.Service.IService;
using TMom.Domain.Model.Entity;
using TMom.Domain.IRepository;
using SqlSugar;
using TMom.Domain.Model;
using System.Linq.Expressions;
using TMom.Infrastructure;
using System.Net;
using MimeKit;
using MailKit.Net.Smtp;
using System.Text;

namespace TMom.Application.Service.Service
{
    public class BaseInfoService : BaseService<BaseInfo, int>, IBaseInfoService
    {
        private readonly IBaseRepository<BaseInfo, int> _dal;
        private readonly IBaseInfoRepository _baseInfoRepository;
        private readonly IUser _user;
        public BaseInfoService(IBaseRepository<BaseInfo, int> dal, IBaseInfoRepository baseInfoRepository, IUser user)
        {
            this._dal = dal;
            base.BaseRepo = dal;
            _baseInfoRepository = baseInfoRepository;
            _user = user;
        }

        #region 模板生成 CRUD

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="whereExp">查询条件表达式 Item1: 主表查询条件表达式, Item2: 导航表查询条件集合</param>
        /// <param name="pageIndex">页标, 默认1</param>
        /// <param name="pageSize">页数, 默认10</param>
        /// <param name="orderByFields">排序字段, 如name asc,age desc</param>
        /// <returns></returns>
        public async Task<PageModel<BaseInfo>> GetWithPage((Expression<Func<BaseInfo, bool>>, List<FormattableString>) whereExp
            , int pageIndex, int pageSize, string orderByFields = "")
        {
            var data = await _baseInfoRepository.QueryPage(whereExp.Item1, pageIndex, pageSize, null, orderByFields);
            return data;
        }

        /// <summary>
        /// 根据主键Id获取单条数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BaseInfo> GetById(int id)
        {
            var entity = await _baseInfoRepository.QueryById(id);
            return entity;
        }

        /// <summary>
        /// 添加一条数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> AddData(BaseInfo entity)
        {
            entity.UpdateCommonFields(_user.Id);
            var id = await _baseInfoRepository.Add(entity);
            return id;
        }

        /// <summary>
        /// 修改全部数据(默认根据主键更新)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>修改的主键Id</returns>
        public async Task<bool> UpdateData(BaseInfo entity)
        {
            entity.UpdateCommonFields(_user.Id, false);
            bool isSuccess = await _baseInfoRepository.Update(entity);
            return isSuccess;
        }

        /// <summary>
        /// 根据主键Id删除数据
        /// </summary>
        /// <param name="ids">多个以逗号分隔</param>
        /// <returns></returns>
        public async Task<bool> DeleteData(string ids)
        {
            List<int> idList = ids.Split(',').Select(id => id.ObjToInt()).ToList();
            bool isSuccess = await _baseInfoRepository.DeleteSoft(x => idList.Contains(x.Id), _user.Id);
            return isSuccess;
        }

        #endregion 模板生成 CRUD

        /// <summary>
        /// 邮件发送测试
        /// </summary>
        public async Task SendMail()
        {
            // 动态生成HTML表格（示例）
            var data = new List<Student>
            {
                new Student { Name = "李四", Score = 88 },
                new Student { Name = "王五", Score = 92 }
            };
            var htmlTable = new StringBuilder();
            htmlTable.AppendLine("<table border='1' style='width: 80%; margin: auto;'>");
            htmlTable.AppendLine("<tr><th>姓名</th><th>成绩</th></tr>");
            foreach (var student in data)
            {
                htmlTable.AppendLine(
                    $@"<tr>
            <td>{WebUtility.HtmlEncode(student.Name)}</td>
            <td>{student.Score}</td>
        </tr>"
                );
            }
            htmlTable.AppendLine("</table>");

            var htmlBody = $@"
            <html>
            <head>
                <style>
                    table {{ border-collapse: collapse; }}
                    th {{ background-color: #f0f0f0; }}
                </style>
            </head>
            <body>
                <h3>成绩单</h3>
                {htmlTable}
                <p>数据截止时间：{DateTime.Now:yyyy-MM-dd}</p>
            </body>
            </html>";


            // 发送邮件
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("教务系统", "13774308285@163.com"));
            message.To.Add(new MailboxAddress("班主任", "13774308285@163.com"));
            message.Subject = "学生成绩报表";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = "请使用支持HTML的邮箱客户端查看完整成绩表。" // 纯文本回退
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync("smtp.163.com", 465, true);
                await client.AuthenticateAsync("13774308285@163.com", "CNpuDb7zACsNJfed");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误：{ex.Message}");
            }

        }
        public class Student
        {
            public string Name { get; set; }
            public int Score { get; set; }
        }
    }
}