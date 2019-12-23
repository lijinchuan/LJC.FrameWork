using LJC.FrameWork.Comm;
using LJC.FrameWork.HttpApi.EntityBuf;
using LJC.FrameWork.Net.HTTP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LJC.FrameWork.HttpApi
{
    public class APIInvokeHandler : APIHandler
    {
        private APIHandler _hander;
        private string _apimethodname;

        public APIInvokeHandler(string apimethodname, APIHandler hander)
        {
            this._apimethodname = apimethodname;
            this._hander = hander;
        }

        private static string Functions(string apiurl)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("var _apiurl='" + apiurl + "'");
            sb.AppendLine(@"function insertAfter(newElement,targetElement) {
                var parent = targetElement.parentNode;
                if (parent.lastChild == targetElement){
                   parent.appendChild(newElement);
                 }
                 else{
                   parent.insertBefore(newElement, targetElement.nextSibling);
                }
            }");
            sb.AppendLine(@"function copyrow(row){
             var pn=row.parentNode;
               while(pn&&pn.tagName&&(pn.tagName!='TR'&&pn.tagName!='tr')){
                 pn=pn.parentNode;
              }
               var tempNode = document.createElement(pn.tagName);
                tempNode.innerHTML = pn.innerHTML;
                insertAfter(tempNode,pn);
            }");

            sb.AppendLine(@"function remrow(row){
                 var pn=row.parentNode;
               while(pn&&pn.tagName&&(pn.tagName!='TR'&&pn.tagName!='tr')){
                 pn=pn.parentNode;
               }
                if(pn.previousSibling||pn.nextSibling)
                    pn.parentNode.removeChild(pn);
            }");

            sb.AppendLine(@"function copytable(row){
               var pn=row.parentNode;
               while(pn&&pn.tagName&&(pn.tagName!='TABLE'&&pn.tagName!='table')){
                 pn=pn.parentNode;
              }
               var tempNode = document.createElement(pn.tagName);
               if(pn.getAttribute('_call')) tempNode.setAttribute('_call',pn.getAttribute('_call'));
               if(pn.getAttribute('_obj')) tempNode.setAttribute('_obj',pn.getAttribute('_obj'));
               if(pn.getAttribute('class')) tempNode.setAttribute('class',pn.getAttribute('class'));
                tempNode.innerHTML = pn.innerHTML;
                insertAfter(tempNode,pn);
            }");

            sb.AppendLine(@"function remtable(row){
                 var pn=row.parentNode;
                 while(pn&&pn.tagName&&(pn.tagName!='TABLE'&&pn.tagName!='table')){
                   pn=pn.parentNode;
                 }
                 if(pn.previousSibling||pn.nextSibling)
                   pn.parentNode.removeChild(pn);
            }");

            sb.AppendLine(@"function clear(container){
                 if(!container)
                    return;
                 var children=container.children;
                 for(var c in children){
                    if(children[c]._tag){
                       children[c]._tag=null;
                    }
                    clear(children[c]);
                 }
              }");

            sb.AppendLine(@"function collection(container){
                 if(!container)
                    return;
                 var children=container.children;
                 for(var c in children){
                    var attr=children[c].getAttribute&&children[c].getAttribute('_call');
                    if(attr){
                      var args=attr.split(',');
                      eval(args[0]+'.call(children[c],args.slice(1))');
                    }
                    collection(children[c]);
                 }
              }");

            sb.AppendLine(@"function submit(){
                 
                 var container=document.getElementById('datacontainer');
                 var children=container.children;
                 for(var c in children){
                    if(children[c]._tag){
                       children[c]._tag=null;
                    }
                    clear(children[c]);
                 }

                 var firsttable=null;
                 for(var c in children){
                    if(!firsttable&&children[c].tagName=='TABLE'&&children[c].getAttribute('_obj')){
                        firsttable=children[c];
                    }
                    var attr=children[c].getAttribute&&children[c].getAttribute('_call');
                    if(attr){
                      var args=attr.split(',');
                      eval(args[0]+'.call(children[c],args.slice(1))');
                    }
                    collection(children[c]);
                 }
                document.getElementById('overlay').style.top='0px';
                 var obj= firsttable?firsttable._tag:{};
                 //document.getElementById(""jsondata"").value=JSON.stringify(obj);
                 document.getElementById('overlay').style.display='block';
                 var top=document.body.scrollTop;
                 if(top>0){
                   document.getElementById('overlay').style.top=top+'px';
                   document.body.style.overflow='hidden';
                 }
                 document.getElementById(""sendjsondata"").value=JSON.stringify(obj,null,2);
                 Ajax.post(_apiurl,JSON.stringify(obj),function fn(data){
                    document.getElementById(""jsondata"").value=data;
                 });
              }");

            sb.AppendLine(@"function settext(args){
                 var pn=this.parentNode;
                 while(pn&&pn.tagName&&(pn.tagName!='TABLE'&&pn.tagName!='table')&&!pn.getAttribute('_obj')){
                   pn=pn.parentNode;
                 }
                 if(pn){
                       if(pn._tag instanceof Array)
                           pn._tag.push(this.value)
                       else
                          (pn._tag=pn._tag||{})[args[0]]=this.value;
                 }
            }");

            sb.AppendLine(@"function setbool(args){
                var pn=this.parentNode;
                 while(pn&&pn.tagName&&(pn.tagName!='TABLE'&&pn.tagName!='table')&&!pn.getAttribute('_obj')){
                   pn=pn.parentNode;
                 }
                 if(pn){
                      if(pn._tag instanceof Array)
                           pn._tag.push(this.checked)
                       else
                       (pn._tag=pn._tag||{})[args[0]]=this.checked;
                 }
            }");

            sb.AppendLine(@"function set(args){
                var pn=this.parentNode;
                 while(pn&&pn.tagName&&(pn.tagName!='TABLE'&&pn.tagName!='table')&&!pn.getAttribute('_obj')){
                   pn=pn.parentNode;
                 }
                 if(pn){
                       if(pn._tag instanceof Array)
                           pn._tag.push(parseFloat(this.value)||0)
                       else
                          (pn._tag=pn._tag||{})[args[0]]=parseFloat(this.value)||0;
                 }
            }");

            sb.AppendLine(@"function setdic(){
               var pn=this.parentNode;
                 while(pn&&pn.tagName&&(pn.tagName!='TABLE'&&pn.tagName!='table')&&!pn.getAttribute('_obj')){
                   pn=pn.parentNode;
                 }
                 if(pn){
                    pn._tag=pn._tag||{};
                    pn._tag[this.previousSibling.previousElementSibling.value]=this.value;
                 }
            }");

            sb.AppendLine(@"function attach(args){
                 this._tag=this._tag||(args[1]=='True'?[]:{});
                 var pn=this.parentNode;
                 while(pn&&pn.tagName&&(pn.tagName!='TABLE'&&pn.tagName!='table')&&!pn.getAttribute('_obj')){
                   pn=pn.parentNode;
                 }

                 if(pn){
                      if(args[0]==''){
                         
                         (pn._tag=(pn._tag&&pn._tag instanceof Array&&pn._tag)||[]).push(this._tag);
                       }else{
                             (pn._tag=pn._tag||{})[args[0]]=this._tag;
                     }
                 }
            }");

            sb.AppendLine(@"function changetab(tabid,pageid,tabid2,pageid2){
                var tab1=document.getElementById(tabid)
                var tab2=document.getElementById(tabid2)
                var page1= document.getElementById(pageid)
                var page2=document.getElementById(pageid2)
                tab1.className='on';
                page1.style.display='';
                tab2.className='';
                page2.style.display='none';
            }");

            sb.AppendLine(@"var Ajax={
  get: function(url, fn) {
    // XMLHttpRequest对象用于在后台与服务器交换数据   
    var xhr = new XMLHttpRequest();            
    xhr.open('GET', url, true);
    xhr.onreadystatechange = function() {
      // readyState == 4说明请求已完成
      if (xhr.readyState == 4 && xhr.status == 200 || xhr.status == 304) { 
        // 从服务器获得数据 
        fn.call(this, xhr.responseText);  
      }
    };
    xhr.send();
  },
  // datat应为'a=a1&b=b1'这种字符串格式，在jq里如果data为对象会自动将对象转成这种字符串格式
  post: function (url, data, fn) {
    var xhr = new XMLHttpRequest();
    xhr.open(""POST"", url, true);
    // 添加http头，发送信息至服务器时内容编码类型
    xhr.setRequestHeader(""Content-Type"", ""application/json"");  
    xhr.onreadystatechange = function() {
      if (xhr.readyState == 4 && (xhr.status == 200 || xhr.status == 304)) {
        fn.call(this, xhr.responseText);
      }
    };
    xhr.send(data);
  }
}");
            sb.AppendLine("document.getElementById(\"close\").onclick = function(){document.getElementById('overlay').style.display ='none';document.body.style.overflow='scroll';}");

            sb.AppendLine("</script>");
            return sb.ToString();
        }

        public override bool Process(HttpServer server, HttpRequest request, HttpResponse response)
        {
            string json = LocalCacheManager<string>.Find(_apimethodname + "_invoke", () =>
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<html>");
                sb.Append(@"<head><title>接口调用</title><style>
.heading1{background:#003366;margin-top:0px; color:#fff; padding-top:5px; line-height:40px; width:100%;display:block;} 
#datacontainer table {
width:98%;
font-family: verdana,arial,sans-serif;
font-size:11px;
color:#333333;
border-width: 1px;
border-color: #ccc;
border-collapse: collapse;
}
#datacontainer table th {
border-width: 1px;
padding: 8px;
border-style: solid;
border-color: #ccc;
background-color: #dedede;
}
#datacontainer table td {
vertical-align:top;
border-width: 1px;
padding: 8px;
border-style: solid;
border-color: #ccc;
background-color: #ffffff;
}
.spanbutton{
  cursor:pointer;
  margin-right:5px;
  font-weight:bold;
  font-size:18px;
  color:#10bd86;

}
h2{margin:0;padding:0;}
#overlay{position:absolute;top:0;left:0;width:100%;height:100%;background: rgba(0, 0, 0, 0.5);display:none;}
#win{position:absolute;top:10%;left:10%;width:80%;height:80%;background:#fff;border:4px solid #ccc;opacity:1;filter:alpha(opacity=100);}
h2{font-size:12px;text-align:right;background:#ccc;border-bottom:3px solid #ccc;padding:5px;}
h2 span{color:#f90;cursor:pointer;background:#fff;border:1px solid #ccc;padding:0 2px;}
#tab1,#tab2 {float:left; width:120px; line-height:25px; margin-right:5px;margin-top:4px; color:#333; background:#eee; text-align:center; cursor:pointer; font-weight:bold;}
#tab1.on,#tab2.on {background:#333; color:#fff;}
textarea {border:0px;}
</style></head>");
                sb.Append("<body style='width:100%;margin:0px;'>");
                sb.Append("<h1 class=\"heading1\">" + (this._hander.ApiMethodProp.Aliname ?? this._apimethodname) + "接口调用</h1>");


                sb.Append("<div id='datacontainer' style=\"width:90%;margin:0 auto;\">");
                EntityBufCore.GetInvokeHtml(_hander._requestType, false, sb);
                //context.Response.Write(string.Format("{0}", sb.ToString()));
                sb.Append("<div style=\"clear:both;\"></div>");
                sb.Append("<div style=\"float:right;margin-top:15px;margin-bottom:50px;padding-right:20px;\"><input type=\"button\" value=\"提 交\" style='width:200px;line-height:30px;' onclick=\"submit()\"/></div>");
                sb.Append("</div>");

                sb.Append("<div id =\"overlay\" >");
                sb.Append("<div id=\"win\"><span id=\"tab1\" onclick=\"changetab('tab1','page1','tab2','page2')\" style='margin-left:10px;'>请求</span><span class='on' onclick=\"changetab('tab2','page2','tab1','page1')\" id=\"tab2\">结果</span><h2><span id =\"close\"> x </span></h2><div id='page1' style=\"width:100%;height:90%;display:none;\"><textarea id=\"sendjsondata\" style=\"width:100%;height:100%;\"></textarea></div><div id='page2' style=\"width:100%;height:90%;\"><textarea id=\"jsondata\" style=\"width:100%;height:100%;\"></textarea></div></div>");
                sb.Append("</div>");

                sb.Append(Functions(new Regex("/invoke$", RegexOptions.IgnoreCase).Replace(request.Url, "")));
                sb.Append("</body>");
                sb.Append("</html>");

                return sb.ToString();
            }, 1440);

            response.Content = json;
            response.ReturnCode = 200;

            return true;
        }
    }
}
