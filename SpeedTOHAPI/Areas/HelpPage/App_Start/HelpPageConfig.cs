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
            #region PatientDemographic
            config.SetSampleResponse(@"<table>
                                           <tr><td colspan=2><b>Status</b> = 200 if successful</td></tr>
                                           <tr><td colspan=2><b>Message</b></td></tr>
                                           <tr><td colspan=2><b>Exception</b></td></tr>
                                           <tr><td colspan=2><b>Data:</b> null</td></tr>
                                           <tr><td colspan=2><b>Error</b>: error list if any</td></tr>
                                           
                                       </table>", new MediaTypeHeaderValue("application/json"), "PatientDemographics", "POSTPatientDemographic");
            config.SetSampleResponse(@"", new MediaTypeHeaderValue("application/xml"), "PatientDemographics", "POSTPatientDemographic");
            config.SetSampleResponse(@"", new MediaTypeHeaderValue("text/xml"), "PatientDemographics", "POSTPatientDemographic");

            config.SetSampleResponse(@"<table>
                                           <tr><td colspan=2><b>Status</b>= 200 if successful</td></tr>
                                           <tr><td colspan=2><b>Message</b></td></tr>
                                           <tr><td colspan=2><b>Exception</b></td></tr>
                                           <tr><td colspan=2><b>Data</b> null</td></tr>
                                           <tr><td colspan=2><b>Error</b>: error list if any</td></tr>
                                          
                                       </table>", new MediaTypeHeaderValue("application/json"), "PatientDemographics", "PUTPatientDemographic");
            config.SetSampleResponse(@"", new MediaTypeHeaderValue("application/xml"), "PatientDemographics", "PUTPatientDemographic");
            config.SetSampleResponse(@"", new MediaTypeHeaderValue("text/xml"), "PatientDemographics", "PUTPatientDemographic");

            config.SetSampleResponse(@"<table>
                                           <tr><td colspan=2><b>Status</b>= 200 if successful</td></tr>
                                           <tr><td colspan=2><b>Message</b></td></tr>
                                           <tr><td colspan=2><b>Exception</b></td></tr>
                                           <tr>
                                                <td colspan=2><b>Data = </b></td>
                                           </tr>
                                            <tr>
                                                <td colspan=2>{ </td>
                                           </tr>
                                           <tr>
                                                <td>PatientID</td>
                                                <td>Mã bệnh nhân của hệ thống</td>
                                           </tr>
                                          <tr>
                                                <td>VisitCode</td>
                                                <td>Mã bệnh nhân</td>
                                           </tr>
                                            <tr>
                                                <td>HN</td>
                                                <td>Hospital Number</td>
                                           </tr>
                                            <tr>
                                                <td>BedCode</td>
                                                <td>Mã giường</td>
                                           </tr>
                                            <tr>
                                                <td>Ward</td>
                                                <td>Phường</td>
                                           </tr>
                                            <tr>
                                                <td>PatientFullName</td>
                                                <td>Họ và tên bệnh nhân</td>
                                           </tr>
                                            <tr>
                                                <td>DoB</td>
                                                <td>Ngày sinh bệnh nhân</td>
                                           </tr>
                                            <tr>
                                                <td>Nationality</td>
                                                <td>Quốc tịch bệnh nhân</td>
                                           </tr>
                                            <tr>
                                                <td>PrimaryDoctor</td>
                                                <td>Bác sĩ phụ trách</td>
                                           </tr>
                                            <tr>
                                                <td>FastingFrom</td>
                                                <td>Kiêng ăn từ ngày</td>
                                           </tr>
                                            <tr>
                                                <td>FastingTo</td>
                                                <td>Kiêng ăn đến hết ngày</td>
                                           </tr>
                                            <tr>
                                                <td>LengthOfStay</td>
                                                <td>Thời gian lưu trú</td>
                                           </tr>
                                            <tr>
                                                <td>PreviousBed</td>
                                                <td>Giường trước</td>
                                           </tr>
                                            <tr>
                                                <td>MovedToBed</td>
                                                <td>Đã chuyển tới giường</td>
                                           </tr>
                                            <tr>
                                                <td>MovedToBed</td>
                                                <td>Đã chuyển tới giường</td>
                                           </tr>
                                            <tr>
                                                <td>DoNotOrderFrom</td>
                                                <td>Đừng gọi món từ ngày</td>
                                           </tr>
                                            <tr>
                                                <td>DoNotOrderTo</td>
                                                <td>Đừng gọi món đến hết ngày</td>
                                           </tr>
                                            <tr>
                                                <td>IsActive</td>
                                                <td>Trạng thái (1: tồn tại, 0: không tồn tại)</td>
                                           </tr>
                                            <tr>
                                                <td>CreatedDate</td>
                                                <td>Ngày tạo trên hệ thống</td>
                                           </tr>
                                            <tr>
                                                <td>ModifiedDate</td>
                                                <td>Ngày cập nhật trên hệ thống</td>
                                           </tr>
                                            <tr>
                                                <td colspan=2><b>}... </b></td>
                                           </tr>
                                       </table>", new MediaTypeHeaderValue("application/json"), "PatientDemographics", "GETPatientDemographic");
            config.SetSampleResponse(@"", new MediaTypeHeaderValue("application/xml"), "PatientDemographics", "GETPatientDemographic");
            config.SetSampleResponse(@"", new MediaTypeHeaderValue("text/xml"), "PatientDemographics", "GETPatientDemographic");
            #endregion
            #region DietaryProperties
            config.SetSampleResponse(@"<table>
                                           <tr><td colspan=2><b>Status</b> = 200 if successful</td></tr>
                                           <tr><td colspan=2><b>Message</b></td></tr>
                                           <tr><td colspan=2><b>Exception</b></td></tr>
                                           <tr><td colspan=2><b>Data:</b> null</td></tr>
                                           <tr><td colspan=2><b>Error</b>: error list if any</td></tr>
                                           
                                       </table>", new MediaTypeHeaderValue("application/json"), "DietaryProperties", "POSTDietaryProperties");
            config.SetSampleResponse(@"", new MediaTypeHeaderValue("application/xml"), "DietaryProperties", "POSTDietaryProperties");
            config.SetSampleResponse(@"", new MediaTypeHeaderValue("text/xml"), "DietaryProperties", "POSTDietaryProperties");

            config.SetSampleResponse(@"<table>
                                           <tr><td colspan=2><b>Status</b>= 200 if successful</td></tr>
                                           <tr><td colspan=2><b>Message</b></td></tr>
                                           <tr><td colspan=2><b>Exception</b></td></tr>
                                           <tr><td colspan=2><b>Data</b> null</td></tr>
                                           <tr><td colspan=2><b>Error</b>: error list if any</td></tr>
                                          
                                       </table>", new MediaTypeHeaderValue("application/json"), "DietaryProperties", "PUTDietaryProperties");
            config.SetSampleResponse(@"", new MediaTypeHeaderValue("application/xml"), "DietaryProperties", "PUTDietaryProperties");
            config.SetSampleResponse(@"", new MediaTypeHeaderValue("text/xml"), "DietaryProperties", "PUTDietaryProperties");

            config.SetSampleResponse(@"<table>
                                           <tr><td colspan=2><b>Status</b>= 200 if successful</td></tr>
                                           <tr><td colspan=2><b>Message</b></td></tr>
                                           <tr><td colspan=2><b>Exception</b></td></tr>
                                           <tr>
                                                <td colspan=2><b>Data = </b></td>
                                           </tr>
                                            <tr>
                                                <td colspan=2>{ </td>
                                           </tr>
                                           <tr>
                                                <td>PatientID</td>
                                                <td>Mã bệnh nhân của hệ thống</td>
                                           </tr>
                                          <tr>
                                                <td>VisitCode</td>
                                                <td>Mã bệnh nhân</td>
                                           </tr>
                                            <tr>
                                                <td>HN</td>
                                                <td>Hospital Number</td>
                                           </tr>
                                            <tr>
                                                <td>FoodTexture</td>
                                                <td>Thực Phẩm Kết Cấu</td>
                                           </tr>
                                            <tr>
                                                <td>Comments</td>
                                                <td>Nội dung bình luận</td>
                                           </tr>
                                            <tr>
                                                <td>KitchenCode</td>
                                                <td>Mã nhà bếp</td>
                                           </tr>
                                            <tr>
                                                <td>KitchenName</td>
                                                <td>Tên nhà bếp</td>
                                           </tr>
                                            <tr>
                                                <td>PantryCode</td>
                                                <td>Mã điều dưỡng</td>
                                           </tr>
                                            <tr>
                                                <td>PantryCode</td>
                                                <td>Tên điều dưỡng</td>
                                           </tr>
                                            <tr>
                                                <td>SnackCode</td>
                                                <td>Mã món ăn nhẹ</td>
                                           </tr>
                                            <tr>
                                                <td>SnackName</td>
                                                <td>Tên món ăn nhẹ</td>
                                           </tr>
                                            <tr>
                                                <td>ValidFrom</td>
                                                <td>Thời gian hiệu lực của đặc tính dinh dưỡng từ ngày </td>
                                           </tr>
                                            <tr>
                                                <td>ValidFrom</td>
                                                <td>Thời gian hiệu lực của đặc tính dinh dưỡng đến hết ngày</td>
                                           </tr>
                                            <tr>
                                                <td>FoodAllergiesList</td>
                                                <td>Danh sách thực phẩm dị ứng</td>
                                           </tr>
                                            <tr>
                                                <td>MenuTypes</td>
                                                <td>Danh sách mã chế độ dinh dưỡng</td>
                                           </tr>
                                           
                                            <tr>
                                                <td>IsActive</td>
                                                <td>Trạng thái (1: tồn tại, 0: không tồn tại)</td>
                                           </tr>
                                            <tr>
                                                <td>CreatedDate</td>
                                                <td>Ngày tạo trên hệ thống</td>
                                           </tr>
                                            <tr>
                                                <td>ModifiedDate</td>
                                                <td>Ngày cập nhật trên hệ thống</td>
                                           </tr>
                                            <tr>
                                                <td colspan=2><b>}... </b></td>
                                           </tr>
                                       </table>", new MediaTypeHeaderValue("application/json"), "DietaryProperties", "GETDietaryProperties");
            config.SetSampleResponse(@"", new MediaTypeHeaderValue("application/xml"), "DietaryProperties", "GETDietaryProperties");
            config.SetSampleResponse(@"", new MediaTypeHeaderValue("text/xml"), "DietaryProperties", "GETDietaryProperties");
            #endregion
            config.SetDocumentationProvider(new XmlDocumentationProvider(HttpContext.Current.Server.MapPath("~/App_Data/SpeedTOHAPI.xml")));
        }

    }
}