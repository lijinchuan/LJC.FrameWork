using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using LJC.FrameWork.Net.POP3;
using LJC.FrameWork.Net.POP3.Client;
using LJC.FrameWork.Net.Mail;
using LJC.FrameWork.Net.SMTP;
using LJC.FrameWork.Net.SMTP.Client;

namespace LJC.Comm.SMTPMail
{
    public class EMail
    {
        public string MailAccount
        {
            get;
            set;
        }

        private string _smtpServer;
        public string SMTPServer
        {
            get
            {
                if (string.IsNullOrEmpty(_smtpServer))
                {
                    if (MailAccount.ToLower().Contains("@sina."))
                        _smtpServer = "smtp.sina.com.cn";
                    else if (MailAccount.ToLower().Contains("@qq."))
                        _smtpServer = "smtp.qq.com";
                    else
                        throw new Exception("找不到电子邮件"+MailAccount+"的发件服务器地址。");
                }

                return _smtpServer;
            }
            set
            {
                _smtpServer = value;
            }
        }

        public int _smtpPort = 25;
        public int SMTPPort
        {
            get
            {
                return _smtpPort;
            }
            set
            {
                _smtpPort = value;
            }
        }

        private string _pop3Server;
        public string Pop3Server
        {
            get
            {
                if(string.IsNullOrEmpty(_pop3Server))
                {
                    if (this.MailAccount.ToLower().Contains("@sina."))
                        _pop3Server = "pop3.sina.com.cn";
                    else if (this.MailAccount.ToLower().Contains("@qq."))
                        _pop3Server = "pop.qq.com";
                    else
                        throw new Exception("找不到电子邮件" + MailAccount + "的收件服务器地址。");
                }

                return _pop3Server;
            }
            set
            {
                _pop3Server = value;
            }
        }

        private int _pop3Port = 110;
        public int Pop3Port
        {
            get
            {

                return _pop3Port;
            }
            set
            {
                _pop3Port = value;
            }
        }

        public string MailPassword
        {
            get;
            set;
        }

        public EMail(string account, string pwd)
        {
            this.MailAccount = account;
            this.MailPassword = pwd;
        }

        /// <summary>
        /// 读后删除
        /// </summary>
        public bool DelAfterRecived
        {
            get;
            set;
        }

        private bool _ssl = false;
        public bool SSL
        {
            get
            {
                return _ssl;
            }
            set
            {
                _ssl = value;
                if (value)
                {
                    _pop3Port = LJC.FrameWork.Net.WellKnownPorts.POP3_SSL;
                }
            }
        }

        public bool SendMail(string _to,string _subject,string _body)
        {
            try
            {
                //SMTP_Client client = new SMTP_Client();
                //client.Connect(SMTPServer, SMTPPort);
                //client.Auth(new FrameWork.Net.AUTH.AUTH_SASL_Client_Login(MailAccount, MailPassword));
                //return client.IsAuthenticated;

                MailMessage mm = new MailMessage(MailAccount, _to);
                mm.Subject = _subject;
                mm.Body = _body;
                mm.BodyEncoding = Encoding.UTF8;
                SmtpClient sc = new SmtpClient();
                //sc.DeliveryMethod=SmtpDeliveryMethod.
                //sc.Host = "smtp.sina.com.cn";
                sc.Host = SMTPServer;
                sc.Port = SMTPPort;
                //sc.Credentials = new NetworkCredential("lulufaer", "ljc123456");
                sc.Credentials = new NetworkCredential(MailAccount, MailPassword);
                //sc.UseDefaultCredentials = true;
                sc.Send(mm);

                return true;
            }
            catch(Exception e)
            {
                LJC.FrameWork.LogManager.Logger.TextLog("发送邮件失败", e, FrameWork.LogManager.LogCategory.Other);
                return false;
            }
        }

        public bool SendHtmlMail(string _to, string _subject, string _body, params Attachment[] attach)
        {
            try
            {
                MailMessage mm = new MailMessage(MailAccount, _to);
                mm.Subject = _subject;
                mm.Body = _body;
                mm.BodyEncoding = Encoding.UTF8;
                mm.IsBodyHtml = true;

                for (int i = 0; i < attach.Length; i++)
                {
                    mm.Attachments.Add(attach[i]);
                }

                SmtpClient sc = new SmtpClient();
                //sc.Host = "smtp.sina.com.cn";
                sc.Host = SMTPServer;
                sc.Port = SMTPPort;
                //sc.Credentials = new NetworkCredential("lulufaer", "ljc123456");
                sc.Credentials = new NetworkCredential(MailAccount, MailPassword);
                sc.Send(mm);

                return true;
            }
            catch (Exception e)
            {
                LJC.FrameWork.LogManager.Logger.TextLog("发送邮件失败", e, FrameWork.LogManager.LogCategory.Other);
                return false;
            }
        }

        public bool SendMail(string _to, string _subject, string _body,params Attachment[] attach)
        {
            try
            {
                MailMessage mm = new MailMessage(MailAccount, _to);
                mm.Subject = _subject;
                mm.Body = _body;
                mm.BodyEncoding = Encoding.UTF8;
                for (int i = 0; i < attach.Length; i++)
                {
                    mm.Attachments.Add(attach[i]);
                }
                SmtpClient sc = new SmtpClient();
                //sc.Host = "smtp.sina.com.cn";
                sc.Host = SMTPServer;
                sc.Port = SMTPPort;
                //sc.Credentials = new NetworkCredential("lulufaer", "ljc123456");
                sc.Credentials = new NetworkCredential(MailAccount, MailPassword);
                
                sc.Send(mm);

                return true;
            }
            catch (Exception e)
            {
                LJC.FrameWork.LogManager.Logger.TextLog("发送邮件失败", e, FrameWork.LogManager.LogCategory.Other);
                return false;
            }
        }

        public List<Mail_Message> ReciveEmail()
        {
            List<Mail_Message> mailMsgs = new List<Mail_Message>();

            POP3_Client pop3 = new POP3_Client();

            //pop3.Connect("pop3.sina.com.cn",110);
            pop3.Connect(Pop3Server, Pop3Port,SSL);
            //pop3.Login("lulufaer", "ljc123456");
            pop3.Login(MailAccount, MailPassword);

            if (pop3.IsAuthenticated)
            {
                for (int i = 0; i < pop3.Messages.Count; i++)
                {
                    //MailMsg msg= new MailMsg();

                    byte[] bytes= pop3.Messages[i].MessageToByte();

                    Mail_Message m_msg = Mail_Message.ParseFromByte(bytes);

                    //msg.msgFrom = m_msg.From[0].Address;
                    //msg.msgTime = m_msg.Date;
                    //msg.msgTo = m_msg.To.Mailboxes[0].Address;

                    //msg.MsgSubject = m_msg.Subject;
                    //msg.MsgContent = m_msg.BodyText;

                    mailMsgs.Add(m_msg);

                    if (DelAfterRecived)
                    {
                        pop3.Messages[i].MarkForDeletion();
                        //pop3.Messages[i].MarkForDeletion();
                        //pop3.Messages[i].MarkForDeletion();
                    }
                    
                }  
            }
            pop3.Disconnect();

            return mailMsgs;
        }


    }
}
