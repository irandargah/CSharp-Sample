using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IranDargah.Controllers
{
    public class PaymentController : ControllerBase
    {
        private const string _createPaymentUrl = "https://dargaah.com/payment";
        private const string _verifyPaymentUrl = "https://dargaah.com/verification";
        private const string _createPaymentSoapUrl = "https://dargaah.com/wsdl";

        // Restful web services

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreatePayment()
        {
            ViewModels.CreatePayment payment = new ViewModels.CreatePayment()
            {
                merchantID = "Test", // Must be fill by you'r merchant ID
                amount = 1000,
                callbackURL = "https://test.com",
                orderId = "1"
            };

            string requestBody = JsonConvert.SerializeObject(payment); // As Json
            byte[] requestBodyByteArray = Encoding.UTF8.GetBytes(requestBody); // As Byte array

            WebRequest request = WebRequest.Create(_createPaymentUrl);
            request.Method = "POST";
            request.Timeout = 60 * 1000; // For 1 min
            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(requestBodyByteArray, 0, requestBodyByteArray.Length);
            }

            ViewModels.CreatePaymentResponse paymentResponse;
            WebResponse response = request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                paymentResponse = JsonConvert.DeserializeObject<ViewModels.CreatePaymentResponse>(reader.ReadToEnd());
            }

            if (paymentResponse == null)
                return BadRequest(new { message = "Error" });

            if (paymentResponse.status == 200)
            {
                string paymentGateWayUrl = "https://dargaah.com/ird/startpay/" + paymentResponse.authority;
                return Redirect(paymentGateWayUrl);
            }

            return BadRequest(new { message = "Error" });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult PaymentCallBack([FromBody] ViewModels.PaymentCallBack model)
        {
            // You must check the parameters data with database data and then store you'r parameters data to database for verify payment.

            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyPayment()
        {
            ViewModels.VerifyPayment verify = new ViewModels.VerifyPayment()
            {
                merchantID = "Test",
                authority = "66452",
                amount = 1000,
                orderId = "1"
            };

            string requestBody = JsonConvert.SerializeObject(verify);
            byte[] requestBodyByteArray = Encoding.UTF8.GetBytes(requestBody);

            WebRequest request = WebRequest.Create(_verifyPaymentUrl);
            request.Method = "POST";
            request.Timeout = 60 * 1000; // For 1 min
            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(requestBodyByteArray, 0, requestBodyByteArray.Length);
            }

            ViewModels.VerifyPaymentResponse verifyReponse;
            WebResponse response = request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                verifyReponse = JsonConvert.DeserializeObject<ViewModels.VerifyPaymentResponse>(reader.ReadToEnd());
            }

            if (verifyReponse == null)
                return BadRequest(new { message = "Error" });

            if (verifyReponse.status == 200)
            {
                // You must store payment detail in you'r database and change payment status to success for user.
                return Ok();
            }

            return Ok();
        }

        // Soap web services

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreatePaymentSoap()
        {
            string requestBodyContent = @"wsdl:definitions xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/wsdl/soap/"" xmlns:tns=""Irandargah"" xmlns:wsdl=""http://schemas.xmlsoap.org/wsdl/"" name=""Irandargah"" targetNamespace=""Irandargah""><xsd:documentation/>"
            + @"<wsdl:types><xsd:schema elementFormDefault=""qualified"" targetNamespace=""Irandargah"">"
            + @"<xsd:element name=""IRDPayment"">"
            + @"<xsd:complexType>"
            + @"<xsd:sequence>"
            + @"<xsd:element minOccurs=""1"" maxOccurs=""1"" name=""merchantID"" type=""s: string"" value=""1"" />"
            + @"<xsd:element minOccurs=""1"" maxOccurs=""1"" name=""amount"" type=""s: int"" value=1000 />"
            + @"<xsd:element minOccurs=""1"" maxOccurs=""1"" name=""callbackURL"" type=""s: string"" value=""https://test.com"" />"
            + @"<xsd:element minOccurs=""1"" maxOccurs=""1"" name=""orderId"" type=""s: string"" value=""1"" />"
            + @"<xsd:element minOccurs=""0"" maxOccurs=""1"" name=""cardNumber"" type=""s: string"" value="""" />"
            + @"<xsd:element minOccurs=""0"" maxOccurs=""1"" name=""mobile"" type=""s: string"" value="""" />"
            + @"<xsd:element minOccurs=""0"" maxOccurs=""1"" name=""description"" type=""s: string"" value="""" />";

            XmlDocument requestBody = new XmlDocument();
            requestBody.LoadXml(requestBodyContent);

            WebRequest request = WebRequest.Create(_createPaymentSoapUrl + " / IRDPayment");
            request.Method = "POST";
            request.Timeout = 60 * 1000; // For 1 min
            request.Headers.Add(@$"SOAPAction:{_createPaymentSoapUrl}/IRDPayment");
            request.ContentType = "text/xml;charset=\"utf-8\"";
            using (Stream writer = request.GetRequestStream())
            {
                requestBody.Save(writer);
            }

            ViewModels.CreatePaymentResponse result;
            WebResponse response = request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                XmlDocument xmlResponse = new XmlDocument();
                xmlResponse.LoadXml(reader.ReadToEnd());
                string requestResponse = JsonConvert.SerializeXmlNode(xmlResponse);
                result = JsonConvert.DeserializeObject<ViewModels.CreatePaymentResponse>(requestResponse);
            }

            if (result == null)
                return BadRequest(new { message = "Error" });

            if (result.status == 200)
                return Redirect("https://dargaah.com/ird/startpay/" + result.authority);

            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult PaymentSoapCallBack([FromBody] string response)
        {
            // You must check the parameters data with database data and then store you'r parameters data to database for verify payment.

            XmlDocument document = new XmlDocument();
            document.LoadXml(response);
            string resulAsJson = JsonConvert.SerializeXmlNode(document);
            ViewModels.CreatePaymentResponse result = JsonConvert.DeserializeObject<ViewModels.CreatePaymentResponse>(resulAsJson);

            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyPaymentSoap()
        {
            string requestBodyContent = @"wsdl:definitions xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/wsdl/soap/"" xmlns:tns=""Irandargah"" xmlns:wsdl=""http://schemas.xmlsoap.org/wsdl/"" name=""Irandargah"" targetNamespace=""Irandargah""><xsd:documentation/>"
            + @"<wsdl:types><xsd:schema elementFormDefault=""qualified"" targetNamespace=""Irandargah"">"
            + @"<xsd:element name=""IRDVerification"">"
            + @"<xsd:complexType>"
            + @"<xsd:sequence>"
            + @"<xsd:element minOccurs=""1"" maxOccurs=""1"" name=""merchantID"" type=""s: string"" value=""Test"" />"
            + @"<xsd:element minOccurs=""1"" maxOccurs=""1"" name=""authority"" type=""s: string"" value=""156255"" />"
            + @"<xsd:element minOccurs=""1"" maxOccurs=""1"" name=""amount"" type=""s: int"" value=""1000"" />"
            + @"<xsd:element minOccurs=""1"" maxOccurs=""1"" name=""orderId"" type=""s: string"" value=""1"" />";

            XmlDocument requestBody = new XmlDocument();
            requestBody.LoadXml(requestBodyContent);

            WebRequest request = WebRequest.Create(_createPaymentSoapUrl + " / IRDVerification");
            request.Method = "POST";
            request.Timeout = 60 * 1000; // For 1 min
            request.Headers.Add(@$"SOAPAction:{_createPaymentSoapUrl}/IRDVerification");
            request.ContentType = "text/xml;charset=\"utf-8\"";
            using (Stream writer = request.GetRequestStream())
            {
                requestBody.Save(writer);
            }

            ViewModels.VerifyPaymentResponse result;
            WebResponse response = request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                XmlDocument xmlResponse = new XmlDocument();
                xmlResponse.LoadXml(reader.ReadToEnd());
                string requestResponse = JsonConvert.SerializeXmlNode(xmlResponse);
                result = JsonConvert.DeserializeObject<ViewModels.VerifyPaymentResponse>(requestResponse);
            }

            if (result == null)
                return BadRequest(new { message = "Error" });

            if (result.status == 200)
            {
                // You must store payment detail in you'r database and change payment status to success for user.
            }

            return Ok();
        }
    }
}
