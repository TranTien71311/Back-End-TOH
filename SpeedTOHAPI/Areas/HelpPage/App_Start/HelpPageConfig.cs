// Uncomment the following to provide samples for PageResult<T>. Must also add the Microsoft.AspNet.WebApi.OData
// package to your project.
////#define Handle_PageResultOfT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using System.Web.Http;
#if Handle_PageResultOfT
using System.Web.Http.OData;
#endif

namespace SpeedTOHAPI.Areas.HelpPage
{
    /// <summary>
    /// Use this class to customize the Help Page.
    /// For example you can set a custom <see cref="System.Web.Http.Description.IDocumentationProvider"/> to supply the documentation
    /// or you can provide the samples for the requests/responses.
    /// </summary>
    public static class HelpPageConfig
    {
        public static void Register(HttpConfiguration config)
        {
            #region Test
            config.SetSampleResponse(@"<table>
                                           <tr><td colspan=2><b>Status</b></td></tr>
                                           <tr><td colspan=2><b>Message</b></td></tr>
                                           <tr><td colspan=2><b>Exception</b></td></tr>
                                           <tr><td colspan=2><b>Data</b></td></tr>
                                           <tr><td>EmpNum</td><td>Mã nhân viên</td></tr>
                                           <tr><td>EmpName</td><td>Họ & Tên nhân viên</td></tr>
                                           <tr><td>EmpLastName</td><td>Tên nhân viên</td></tr>
                                           <tr><td>StartWork</td><td>Thời gian bắt đầu làm việc</td></tr>
                                           <tr><td>EndWork</td><td>Thời gian kết thúc làm việc</td></tr>
                                           <tr><td>PosName</td><td>Tên nhân viên hiển thị trên POS</td></tr>
                                           <tr><td>JobPosNum</td><td>Mã chức vụ</td></tr>
                                           <tr><td>JobPosName</td><td>Mô tả chức vụ</td></tr>
                                           <tr><td>IsActive</td><td>Trạng thái (0: Ngừng hoạt động, 1: Đang hoạt động)</td></tr>
                                       </table>", new MediaTypeHeaderValue("application/json"), "Test", "GetEmployeeBySwipe");
            config.SetSampleResponse(@"", new MediaTypeHeaderValue("application/xml"), "Test", "GetEmployeeBySwipe");
            config.SetSampleResponse(@"", new MediaTypeHeaderValue("text/xml"), "Test", "GetEmployeeBySwipe");
            #endregion
        }

    }
}