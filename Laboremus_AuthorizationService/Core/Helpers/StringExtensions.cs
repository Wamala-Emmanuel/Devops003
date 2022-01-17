using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Laboremus_AuthorizationService.Core.Helpers
{
    public static class StringExtensions
    {

        public static string CapitalizeFirstLetter(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }
            var charArray = data.ToCharArray();
            charArray[0] = char.ToUpper(charArray[0]);
            return new string(charArray);
        }
        
        /// <summary>
         /// Returns the value of a given claim. Otherwise null will be returned.
         /// </summary>
         /// <param name="claims"></param>
         /// <param name="claimType"></param>
         /// <returns></returns>
        public static string TryGetClaimValue(List<Claim> claims, string claimType)
        {
            var claim = claims.FirstOrDefault(c => c.Type == claimType);
            return claim?.Value;
        }

        /// <summary>
        /// converts a string into a byte array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] GetByteArrayFromString(this string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        public static string GetResetPasswordTemplate()
        {
            return @"<span class='preheader' style='color: transparent; display: none; height: 0; max - height: 0; max - width: 0; opacity: 0; overflow: hidden; mso - hide: all; visibility: hidden; width: 0;'>This is preheader text. Some clients will show this text as a preview.</span>
                          <table border='0' cellpadding='0' cellspacing ='0' class='body' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background-color: #f6f6f6;'>
                          <tr>
                            <td style='font-family: sans-serif; font-size: 14px; vertical-align: top;'> &nbsp;</td>
                            <td class='container' style='font-family: sans-serif; font-size: 14px; vertical-align: top; display: block; Margin: 0 auto; max-width: 580px; padding: 10px; width: 580px;'>
                              <div class='content' style='box-sizing: border-box; display: block; Margin: 0 auto; max-width: 580px; padding: 10px;'>
                                <table class='main' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background: #ffffff; border-radius: 3px;'>
                                  <tr>
                                    <td class='wrapper' style='font-family: sans-serif; font-size: 14px; vertical-align: top; box-sizing: border-box; padding: 20px;'>
                                      <table border='0' cellpadding='0' cellspacing='0' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;'>
                                        <tr>
                                          <td style='font-family: sans-serif; font-size: 14px; vertical-align: top;'>
                                            <p style='font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 30px;'>Hi there,</p>
                                            <p style ='font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;' > We received a request to reset your password for your ID Verification System account.</p>
                                            <p style ='font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 30px;'> Click the link below to get started:</p>
                                            <table border='0' cellpadding='0' cellspacing='0' class='btn btn-primary' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; box-sizing: border-box;'>
                                              <tbody>
                                                <tr>
                                                  <td align='left' style='font-family: sans-serif; font-size: 14px; vertical-align: top; padding-bottom: 15px;'>
                                                    <table border='0' cellpadding='0' cellspacing='0' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;'>
                                                      <tbody>
                                                        <tr>
                                                          <td style='font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #0d47a1; border-radius: 5px; text-align: center;'> <a href='{resetPasswordLink}' target='_blank' style='display: inline-block; color: #ffffff; background-color: #0d47a1; border: solid 1px #0d47a1; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 12px 25px; text-transform: capitalize; border-color: #0d47a1;'>
                                                            Reset Password</a> </td>
                                                        </tr>
                                                      </tbody>
                                                    </table>
                                                  </td>
                                                </tr>
                                              </tbody>
                                            </table>
                                            <p style='font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 30px;'></p>
                                            <p style='font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;'> Your link will expire in 24 hours.</p>
                                            <p style='font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;'> If you didn't mean to reset your password, simply ignore this email, your password will not change.</p>
                                            <p style='font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;'> Thank you</p>
                                          </td>
                                        </tr>
                                      </table>
                                    </td>
                                  </tr>
                                </table>
                                <div class='footer' style='clear: both; Margin-top: 10px; text-align: center; width: 100%;'>
                                  <table border='0' cellpadding='0' cellspacing='0' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;'>
                                    <tr>
                                      <td class='content-block' style='font-family: sans-serif; vertical-align: top; padding-bottom: 10px; padding-top: 10px; font-size: 12px; color: #999999; text-align: center;'>
                                        <span class='apple-link' style='color: #999999; font-size: 12px; text-align: center;'>ID Verification System</span>
                                      </td>
                                    </tr>
                                    <tr>
                                    </tr>
                                  </table>
                                </div>
                              </div>
                            </td>
                            <td style='font-family: sans-serif; font-size: 14px; vertical-align: top;' > &nbsp;</td>
                          </tr>
                        </table>";
        }
    }
}
