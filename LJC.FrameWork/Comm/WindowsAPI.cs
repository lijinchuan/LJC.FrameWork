﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LJC.FrameWork.Comm
{
    public delegate bool EnumWindProc(IntPtr hWnd, IntPtr lParam);
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width
        {
            get
            {
                return Right - Left;
            }
        }

        public int Hight
        {
            get
            {
                return Bottom - Top;
            }
        }
    }

    public static class WindowsAPI
    {
        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_RELEASE = 0x8000;

        public const uint MEM_RESERVE = 0x2000;
        public const uint PAGE_READWRITE = 4;
        public const uint PROCESS_VM_OPERATION = 0x0008;
        public const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
        public const uint PROCESS_VM_READ = 0x0010;
        public const uint PROCESS_VM_WRITE = 0x0020;

        public const int GW_OWNER = 4;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TOOLWINDOW = 0x00000080;

        //<summary>
        //创建一个窗口
        //</summary>
        public const int WM_CREATE = 0x01;
        //<summary>
        //当一个窗口被破坏时发送
        //</summary>
        public const int WM_DESTROY = 0x02;
        //<summary>
        //移动一个窗口
        //</summary>
        public const int WM_MOVE = 0x03;
        //<summary>
        //改变一个窗口的大小
        //</summary>
        public const int WM_SIZE = 0x05;
        //<summary>
        //一个窗口被激活或失去激活状态
        //</summary>
        public const int WM_ACTIVATE = 0x06;
        //<summary>
        //一个窗口获得焦点
        //</summary>
        public const int WM_SETFOCUS = 0x07;
        //<summary>
        //一个窗口失去焦点
        //</summary>
        public const int WM_KILLFOCUS = 0x08;
        //<summary>
        //一个窗口改变成Enable状态
        //</summary>
        public const int WM_ENABLE = 0x0A;
        //<summary>
        //设置窗口是否能重画
        //</summary>
        public const int WM_SETREDRAW = 0x0B;
        //<summary>
        //应用程序发送此消息来设置一个窗口的文本
        //</summary>
        public const int WM_SETTEXT = 0x0C;
        //<summary>
        //应用程序发送此消息来复制对应窗口的文本到缓冲区
        //</summary>
        public const int WM_GETTEXT = 0x0D;
        //<summary>
        //得到与一个窗口有关的文本的长度（不包含空字符）
        //</summary>
        public const int WM_GETTEXTLENGTH = 0x0E;
        //<summary>
        //要求一个窗口重画自己
        //</summary>
        public const int WM_PAINT = 0x0F;
        //<summary>
        //当一个窗口或应用程序要关闭时发送一个信号
        //</summary>
        public const int WM_CLOSE = 0x10;
        //<summary>
        //当用户选择结束对话框或程序自己调用ExitWindows函数
        //</summary>
        public const int WM_QUERYENDSESSION = 0x11;
        //<summary>
        //用来结束程序运行
        //</summary>
        public const int WM_QUIT = 0x12;
        //<summary>
        //当用户窗口恢复以前的大小位置时，把此消息发送给某个图标
        //</summary>
        public const int WM_QUERYOPEN = 0x13;
        //<summary>
        //当窗口背景必须被擦除时（例在窗口改变大小时）
        //</summary>
        public const int WM_ERASEBKGND = 0x14;
        //<summary>
        //当系统颜色改变时，发送此消息给所有顶级窗口
        //</summary>
        public const int WM_SYSCOLORCHANGE = 0x15;
        //<summary>
        //当系统进程发出WM_QUERYENDSESSION消息后，此消息发送给应用程序，通知它对话是否结束
        //</summary>
        public const int WM_ENDSESSION = 0x16;
        //<summary>
        //当隐藏或显示窗口是发送此消息给这个窗口
        //</summary>
        public const int WM_SHOWWINDOW = 0x18;
        //<summary>
        //发此消息给应用程序哪个窗口是激活的，哪个是非激活的
        //</summary>
        public const int WM_ACTIVATEAPP = 0x1C;
        //<summary>
        //当系统的字体资源库变化时发送此消息给所有顶级窗口
        //</summary>
        public const int WM_FONTCHANGE = 0x1D;
        //<summary>
        //当系统的时间变化时发送此消息给所有顶级窗口
        //</summary>
        public const int WM_TIMECHANGE = 0x1E;
        //<summary>
        //发送此消息来取消某种正在进行的摸态（操作）
        //</summary>
        public const int WM_CANCELMODE = 0x1F;
        //<summary>
        //如果鼠标引起光标在某个窗口中移动且鼠标输入没有被捕获时，就发消息给某个窗口
        //</summary>
        public const int WM_SETCURSOR = 0x20;
        //<summary>
        //当光标在某个非激活的窗口中而用户正按着鼠标的某个键发送此消息给
        //</summary>
        //<summary>
        //当前窗口
        //</summary>
        public const int WM_MOUSEACTIVATE = 0x21;
        //<summary>
        //发送此消息给MDI子窗口
        //</summary>
        //<summary>
        //当用户点击此窗口的标题栏，或
        //</summary>
        //<summary>
        //当窗口被激活，移动，改变大小
        //</summary>
        public const int WM_CHILDACTIVATE = 0x22;
        //<summary>
        //此消息由基于计算机的训练程序发送，通过WH_JOURNALPALYBACK的hook程序分离出用户输入消息
        //</summary>
        public const int WM_QUEUESYNC = 0x23;
        //<summary>
        //此消息发送给窗口当它将要改变大小或位置
        //</summary>
        public const int WM_GETMINMAXINFO = 0x24;
        //<summary>
        //发送给最小化窗口当它图标将要被重画
        //</summary>
        public const int WM_PAINTICON = 0x26;
        //<summary>
        //此消息发送给某个最小化窗口，仅
        //</summary>
        //<summary>
        //当它在画图标前它的背景必须被重画
        //</summary>
        public const int WM_ICONERASEBKGND = 0x27;
        //<summary>
        //发送此消息给一个对话框程序去更改焦点位置
        //</summary>
        public const int WM_NEXTDLGCTL = 0x28;
        //<summary>
        //每当打印管理列队增加或减少一条作业时发出此消息
        //</summary>
        public const int WM_SPOOLERSTATUS = 0x2A;
        //<summary>
        //当button，combobox，listbox，menu的可视外观改变时发送
        //</summary>
        public const int WM_DRAWITEM = 0x2B;
        //<summary>
        //当button, combo box, list box, list view control, or menu item 被创建时
        //</summary>
        public const int WM_MEASUREITEM = 0x2C;
        //<summary>
        //此消息有一个LBS_WANTKEYBOARDINPUT风格的发出给它的所有者来响应WM_KEYDOWN消息
        //</summary>
        public const int WM_VKEYTOITEM = 0x2E;
        //<summary>
        //此消息由一个LBS_WANTKEYBOARDINPUT风格的列表框发送给他的所有者来响应WM_CHAR消息
        //</summary>
        public const int WM_CHARTOITEM = 0x2F;
        //<summary>
        //当绘制文本时程序发送此消息得到控件要用的颜色
        //</summary>
        public const int WM_SETFONT = 0x30;
        //<summary>
        //应用程序发送此消息得到当前控件绘制文本的字体
        //</summary>
        public const int WM_GETFONT = 0x31;
        //<summary>
        //应用程序发送此消息让一个窗口与一个热键相关连
        //</summary>
        public const int WM_SETHOTKEY = 0x32;
        //<summary>
        //应用程序发送此消息来判断热键与某个窗口是否有关联
        //</summary>
        public const int WM_GETHOTKEY = 0x33;
        //<summary>
        //此消息发送给最小化窗口，当此窗口将要被拖放而它的类中没有定义图标，应用程序能返回一个图标或光标的句柄，当用户拖放图标时系统显示这个图标或光标
        //</summary>
        public const int WM_QUERYDRAGICON = 0x37;
        //<summary>
        //发送此消息来判定combobox或listbox新增加的项的相对位置
        //</summary>
        public const int WM_COMPAREITEM = 0x39;
        //<summary>
        //显示内存已经很少了
        //</summary>
        public const int WM_COMPACTING = 0x41;
        //<summary>
        //发送此消息给那个窗口的大小和位置将要被改变时，来调用setwindowpos函数或其它窗口管理函数
        //</summary>
        public const int WM_WINDOWPOSCHANGING = 0x46;
        //<summary>
        //发送此消息给那个窗口的大小和位置已经被改变时，来调用setwindowpos函数或其它窗口管理函数
        //</summary>
        public const int WM_WINDOWPOSCHANGED = 0x47;
        //<summary>
        //当系统将要进入暂停状态时发送此消息
        //</summary>
        public const int WM_POWER = 0x48;
        //<summary>
        //当一个应用程序传递数据给另一个应用程序时发送此消息
        //</summary>
        public const int WM_COPYDATA = 0x4A;
        //<summary>
        //当某个用户取消程序日志激活状态，提交此消息给程序
        //</summary>
        public const int WM_CANCELJOURNA = 0x4B;
        //<summary>
        //当某个控件的某个事件已经发生或这个控件需要得到一些信息时，发送此消息给它的父窗口
        //</summary>
        public const int WM_NOTIFY = 0x4E;
        //<summary>
        //当用户选择某种输入语言，或输入语言的热键改变
        //</summary>
        public const int WM_INPUTLANGCHANGEREQUEST = 0x50;
        //<summary>
        //当平台现场已经被改变后发送此消息给受影响的最顶级窗口
        //</summary>
        public const int WM_INPUTLANGCHANGE = 0x51;
        //<summary>
        //当程序已经初始化windows帮助例程时发送此消息给应用程序
        //</summary>
        public const int WM_TCARD = 0x52;
        //<summary>
        //此消息显示用户按下了F1，如果某个菜单是激活的，就发送此消息个此窗口关联的菜单，否则就发送给有焦点的窗口，如果
        //</summary>
        //<summary>
        //当前都没有焦点，就把此消息发送给
        //</summary>
        //<summary>
        //当前激活的窗口
        //</summary>
        public const int WM_HELP = 0x53;
        //<summary>
        //当用户已经登入或退出后发送此消息给所有的窗口，
        //</summary>
        //<summary>
        //当用户登入或退出时系统更新用户的具体设置信息，在用户更新设置时系统马上发送此消息
        //</summary>
        public const int WM_USERCHANGED = 0x54;
        //<summary>
        //公用控件，自定义控件和他们的父窗口通过此消息来判断控件是使用ANSI还是UNICODE结构
        //</summary>
        public const int WM_NOTIFYFORMAT = 0x55;
        //<summary>
        //当用户某个窗口中点击了一下右键就发送此消息给这个窗口
        //</summary>
        //<summary>
        //
        //</summary>
        public const int WM_CONTEXTMENU = 0x007B;
        //<summary>
        //当调用SETWINDOWLONG函数将要改变一个或多个 窗口的风格时发送此消息给那个窗口
        //</summary>
        public const int WM_STYLECHANGING = 0x7C;
        //<summary>
        //当调用SETWINDOWLONG函数一个或多个 窗口的风格后发送此消息给那个窗口
        //</summary>
        public const int WM_STYLECHANGED = 0x7D;
        //<summary>
        //当显示器的分辨率改变后发送此消息给所有的窗口
        //</summary>
        public const int WM_DISPLAYCHANGE = 0x7E;
        //<summary>
        //此消息发送给某个窗口来返回与某个窗口有关连的大图标或小图标的句柄
        //</summary>
        public const int WM_GETICON = 0x7F;
        //<summary>
        //程序发送此消息让一个新的大图标或小图标与某个窗口关联
        //</summary>
        public const int WM_SETICON = 0x80;
        //<summary>
        //当某个窗口第一次被创建时，此消息在WM_CREATE消息发送前发送
        //</summary>
        public const int WM_NCCREATE = 0x81;
        //<summary>
        //此消息通知某个窗口，非客户区正在销毁
        //</summary>
        public const int WM_NCDESTROY = 0x82;
        //<summary>
        //当某个窗口的客户区域必须被核算时发送此消息
        //</summary>
        public const int WM_NCCALCSIZE = 0x83;
        //<summary>
        //移动鼠标，按住或释放鼠标时发生
        //</summary>
        public const int WM_NCHITTEST = 0x84;
        //<summary>
        //程序发送此消息给某个窗口当它（窗口）的框架必须被绘制时
        //</summary>
        public const int WM_NCPAINT = 0x85;
        //<summary>
        //此消息发送给某个窗口仅当它的非客户区需要被改变来显示是激活还是非激活状态
        //</summary>
        public const int WM_NCACTIVATE = 0x86;
        //<summary>
        //发送此消息给某个与对话框程序关联的控件，widdows控制方位键和TAB键使输入进入此控件通过应
        //</summary>
        public const int WM_GETDLGCODE = 0x87;
        //<summary>
        //当光标在一个窗口的非客户区内移动时发送此消息给这个窗口 非客户区为：窗体的标题栏及窗 的边框体
        //</summary>
        public const int WM_NCMOUSEMOVE = 0xA0;
        //<summary>
        //当光标在一个窗口的非客户区同时按下鼠标左键时提交此消息
        //</summary>
        public const int WM_NCLBUTTONDOWN = 0xA1;
        //<summary>
        //当用户释放鼠标左键同时光标某个窗口在非客户区十发送此消息
        //</summary>
        public const int WM_NCLBUTTONUP = 0xA2;
        //<summary>
        //当用户双击鼠标左键同时光标某个窗口在非客户区十发送此消息
        //</summary>
        public const int WM_NCLBUTTONDBLCLK = 0xA3;
        //<summary>
        //当用户按下鼠标右键同时光标又在窗口的非客户区时发送此消息
        //</summary>
        public const int WM_NCRBUTTONDOWN = 0xA4;
        //<summary>
        //当用户释放鼠标右键同时光标又在窗口的非客户区时发送此消息
        //</summary>
        public const int WM_NCRBUTTONUP = 0xA5;
        //<summary>
        //当用户双击鼠标右键同时光标某个窗口在非客户区十发送此消息
        //</summary>
        public const int WM_NCRBUTTONDBLCLK = 0xA6;
        //<summary>
        //当用户按下鼠标中键同时光标又在窗口的非客户区时发送此消息
        //</summary>
        public const int WM_NCMBUTTONDOWN = 0xA7;
        //<summary>
        //当用户释放鼠标中键同时光标又在窗口的非客户区时发送此消息
        //</summary>
        public const int WM_NCMBUTTONUP = 0xA8;
        //<summary>
        //当用户双击鼠标中键同时光标又在窗口的非客户区时发送此消息
        //</summary>
        public const int WM_NCMBUTTONDBLCLK = 0xA9;
        //<summary>
        //WM_KEYDOWN 按下一个键
        //</summary>
        public const int WM_KEYDOWN = 0x0100;
        //<summary>
        //释放一个键
        //</summary>
        public const int WM_KEYUP = 0x0101;
        //<summary>
        //按下某键，并已发出WM_KEYDOWN， WM_KEYUP消息
        //</summary>
        public const int WM_CHAR = 0x102;
        //<summary>
        //当用translatemessage函数翻译WM_KEYUP消息时发送此消息给拥有焦点的窗口
        //</summary>
        public const int WM_DEADCHAR = 0x103;
        //<summary>
        //当用户按住ALT键同时按下其它键时提交此消息给拥有焦点的窗口
        //</summary>
        public const int WM_SYSKEYDOWN = 0x104;
        //<summary>
        //当用户释放一个键同时ALT 键还按着时提交此消息给拥有焦点的窗口
        //</summary>
        public const int WM_SYSKEYUP = 0x105;
        //<summary>
        //当WM_SYSKEYDOWN消息被TRANSLATEMESSAGE函数翻译后提交此消息给拥有焦点的窗口
        //</summary>
        public const int WM_SYSCHAR = 0x106;
        //<summary>
        //当WM_SYSKEYDOWN消息被TRANSLATEMESSAGE函数翻译后发送此消息给拥有焦点的窗口
        //</summary>
        public const int WM_SYSDEADCHAR = 0x107;
        //<summary>
        //在一个对话框程序被显示前发送此消息给它，通常用此消息初始化控件和执行其它任务
        //</summary>
        public const int WM_INITDIALOG = 0x110;
        //<summary>
        //当用户选择一条菜单命令项或当某个控件发送一条消息给它的父窗口，一个快捷键被翻译
        //</summary>
        public const int WM_COMMAND = 0x111;
        //<summary>
        //当用户选择窗口菜单的一条命令或
        //</summary>
        //<summary>
        //当用户选择最大化或最小化时那个窗口会收到此消息
        //</summary>
        public const int WM_SYSCOMMAND = 0x112;
        public const int SC_MINIMIZE = 0xF020;
        //<summary>
        //最小化
        //</summary>
        public const int SC_CLOSE = 0xF060;
        //<summary>
        //最大化
        //</summary>
        //<summary>
        //发生了定时器事件
        //</summary>
        public const int WM_TIMER = 0x113;
        //<summary>
        //当一个窗口标准水平滚动条产生一个滚动事件时发送此消息给那个窗口，也发送给拥有它的控件
        //</summary>
        public const int WM_HSCROLL = 0x114;
        //<summary>
        //当一个窗口标准垂直滚动条产生一个滚动事件时发送此消息给那个窗口也，发送给拥有它的控件
        //</summary>
        public const int WM_VSCROLL = 0x115;
        //<summary>
        //当一个菜单将要被激活时发送此消息，它发生在用户菜单条中的某项或按下某个菜单键，它允许程序在显示前更改菜单
        //</summary>
        public const int WM_INITMENU = 0x116;
        //<summary>
        //当一个下拉菜单或子菜单将要被激活时发送此消息，它允许程序在它显示前更改菜单，而不要改变全部
        //</summary>
        public const int WM_INITMENUPOPUP = 0x117;
        //<summary>
        //当用户选择一条菜单项时发送此消息给菜单的所有者（一般是窗口）
        //</summary>
        public const int WM_MENUSELECT = 0x11F;
        //<summary>
        //当菜单已被激活用户按下了某个键（不同于加速键），发送此消息给菜单的所有者
        //</summary>
        public const int WM_MENUCHAR = 0x120;
        //<summary>
        //当一个模态对话框或菜单进入空载状态时发送此消息给它的所有者，一个模态对话框或菜单进入空载状态就是在处理完一条或几条先前的消息后没有消息它的列队中等待
        //</summary>
        public const int WM_ENTERIDLE = 0x121;
        //<summary>
        //在windows绘制消息框前发送此消息给消息框的所有者窗口，通过响应这条消息，所有者窗口可以通过使用给定的相关显示设备的句柄来设置消息框的文本和背景颜色
        //</summary>
        public const int WM_CTLCOLORMSGBOX = 0x132;
        //<summary>
        //当一个编辑型控件将要被绘制时发送此消息给它的父窗口通过响应这条消息，所有者窗口可以通过使用给定的相关显示设备的句柄来设置编辑框的文本和背景颜色
        //</summary>
        public const int WM_CTLCOLOREDIT = 0x133;
        //<summary>
        //当一个列表框控件将要被绘制前发送此消息给它的父窗口通过响应这条消息，所有者窗口可以通过使用给定的相关显示设备的句柄来设置列表框的文本和背景颜色
        //</summary>
        public const int WM_CTLCOLORLISTBOX = 0x134;
        //<summary>
        //当一个按钮控件将要被绘制时发送此消息给它的父窗口通过响应这条消息，所有者窗口可以通过使用给定的相关显示设备的句柄来设置按纽的文本和背景颜色
        //</summary>
        public const int WM_CTLCOLORBTN = 0x135;
        //<summary>
        //当一个对话框控件将要被绘制前发送此消息给它的父窗口通过响应这条消息，所有者窗口可以通过使用给定的相关显示设备的句柄来设置对话框的文本背景颜色
        //</summary>
        public const int WM_CTLCOLORDLG = 0x136;
        //<summary>
        //当一个滚动条控件将要被绘制时发送此消息给它的父窗口通过响应这条消息，所有者窗口可以通过使用给定的相关显示设备的句柄来设置滚动条的背景颜色
        //</summary>
        public const int WM_CTLCOLORSCROLLBAR = 0x137;
        //<summary>
        //当一个静态控件将要被绘制时发送此消息给它的父窗口通过响应这条消息，所有者窗口可以 通过使用给定的相关显示设备的句柄来设置静态控件的文本和背景颜色
        //</summary>
        public const int WM_CTLCOLORSTATIC = 0x138;
        //<summary>
        //当鼠标轮子转动时发送此消息个当前有焦点的控件
        //</summary>
        public const int WM_MOUSEWHEEL = 0x20A;
        //<summary>
        //双击鼠标中键
        //</summary>
        public const int WM_MBUTTONDBLCLK = 0x209;
        //<summary>
        //释放鼠标中键
        //</summary>
        public const int WM_MBUTTONUP = 0x208;
        //<summary>
        //移动鼠标时发生，同WM_MOUSEFIRST
        //</summary>
        public const int WM_MOUSEMOVE = 0x200;
        //<summary>
        //按下鼠标左键
        //</summary>
        public const int WM_LBUTTONDOWN = 0x201;
        //<summary>
        //释放鼠标左键
        //</summary>
        public const int WM_LBUTTONUP = 0x202;
        //<summary>
        //双击鼠标左键
        //</summary>
        public const int WM_LBUTTONDBLCLK = 0x203;
        //<summary>
        //按下鼠标右键
        //</summary>
        public const int WM_RBUTTONDOWN = 0x204;
        //<summary>
        //释放鼠标右键
        //</summary>
        public const int WM_RBUTTONUP = 0x205;
        //<summary>
        //双击鼠标右键
        //</summary>
        public const int WM_RBUTTONDBLCLK = 0x206;
        //<summary>
        //按下鼠标中键
        //</summary>
        public const int WM_MBUTTONDOWN = 0x207;
        public const int WM_USER = 0x0400;
        public const int MK_LBUTTON = 0x0001;
        public const int MK_RBUTTON = 0x0002;
        public const int MK_SHIFT = 0x0004;
        public const int MK_CONTROL = 0x0008;
        public const int MK_MBUTTON = 0x0010;
        public const int MK_XBUTTON1 = 0x0020;
        public const int MK_XBUTTON2 = 0x0040;


        /// <summary>
        /// 函数返回与指定字符创相匹配的窗口类名或窗口名的最顶层窗口的窗口句柄。这个函数不会查找子窗口 
        /// </summary>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// 该函数获得一个窗口的句柄，该窗口的类名和窗口名与给定的字符串相匹配。这个函数查找子窗口，从排在给定的子窗口后面的下一个子窗口开始。在查找时不区分大小写。 
        /// </summary>
        /// <param name="parentHandle"></param>
        /// <param name="childAfter"></param>
        /// <param name="className"></param>
        /// <param name="windowTitle"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
        public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);
        [DllImport("kernel32.dll")]
        public static extern long GetLastError();
        [DllImport("user32.dll")]
        public static extern void EnumWindows(EnumWindProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern long GetWindowText(IntPtr hWnd, StringBuilder sb, int count);
        /// <summary>
        /// 枚举一个父窗口的所有子窗口。IntPtr window窗口句柄 
        /// WNDENUMPROC lpEnumFunc 回调函数的地址 
        /// LPARAM lParam 自定义的参数 
        /// 注意：回调函数的返回值将会影响到这个API函数的行为。如果回调函数返回true，则枚举继续直到枚举完成；如果返回false，则将会中止枚举。 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="proc"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr EnumChildWindows(IntPtr hWnd, EnumWindProc proc, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr EnumThreadWindows(int dwThreadId, EnumWindProc proc, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);
        [DllImport("user32.dll")]
        public static extern bool SetWindowText(IntPtr hWnd, string text);
        [DllImport("user32.dll")]
        public static extern bool SetActiveWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool SetFocus(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern int SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
        [DllImport("user32.dll")]
        public static extern IntPtr SetCapture(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool UpdateWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hWnd, int rect, bool earse);
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, StringBuilder lParam);
        [DllImport("user32.DLL")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        [DllImport("user32.DLL")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll")]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, ref uint vNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, ref uint vNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, ref uint vNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, ref uint vNumberOfBytesRead);
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint dwProcessId);
        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool GetWindow(IntPtr HWND, int GW_OWNER);
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr HWND);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetClassName(IntPtr HWND, StringBuilder lpClassName, int nCount);
        [DllImport("user32.dll")]
        public static extern bool GetWindowLong(IntPtr HWND, int nIndex);
        /// <summary>
        /// 该函数获得指定窗口所属的类的类名。 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern int SendTextMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        //消息发送API 
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(IntPtr hWnd, // 信息发往的窗口的句柄 
            int Msg, // 消息ID 
            int wParam, // 参数1 
            int lParam //参数2 
            );
    }
}
