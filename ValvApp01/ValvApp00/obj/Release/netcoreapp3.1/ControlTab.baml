   M S B A M L   `   `   `  ����   	ValvApp00    ValvApp00.ControlTab   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     -   
�������.���   � ColumnDefinitions  �� $��1* ��� $��2* ��� $��3* �.+�� $��0�� � RowDefinitions �� $��3* ��� $��1* �.+��� -    ��Name$ 
controlBox��$��Control��$��10,10,10,10q�$��0��.���� .+��� -    ��Name$ 
scanbutton��$
��Scan��$��10,10,10,10q��� -   $ connectbutton��$��Connect��$��10,10,10,10q��� -   $ synctime_button��$��	Sync Time��$��10,10,10,10q��� -    ��Name$ led_checkBox��$	��LED��$��Center=��� -   $ startlog_button��$��	Start log��$��10,10,10,10q��� -   $ stoplog_button��$��Stop log��$��10,10,10,10q��� -	   $ deploy_checkBox��$��Deploy��$��Center=��� -
   $ statsBox��$��Stats��$��10,10,10,10q�$��1��.���� .+��� -    ��Name$ 	boardtime������ *Btime+ �� Mode$ OneWay=��� -   $ 	vbatvalue������ *Vbat+$ OneWay=�� $��1��.+��� -   $ filesBox��$��Files��$��10,10,10,10q�.���  �� $��1* ��� $��4* �.+��� .+��� -   $ refresh_button��$��Refresh��$��10,10,10,10q��� -   $ format_button��$��Format��$��10,10,10,10q��� -   $ download_button��$��Download��$��10,10,10,10q��� -   $��file_ListView��$��1��$��10,10,10,10q� �� SelectedItem �� *SelectedFile+$ TwoWay=� �� View �� .P��� $
��Name��	 �� Width$		 120��
 �� DisplayMemberBinding
 �� *name+�� $
��Date��$		 150��
 �� *
dateTime+�� $
��Size��$	 50��
 �� *size+ � ContextMenu �� .@�m� -    m�Name$ download_menuItem��$��Download��m� -   $ delete_menuItem��$��Delete��� $��2��$��10,20,10,10q�.+��� -    ��Name$ 	statusBox�� �� TextWrapping$
 Wrap=�" �� VerticalScrollBarVisibility$ Visible=����� *
Stat_str+$ OneWay=�