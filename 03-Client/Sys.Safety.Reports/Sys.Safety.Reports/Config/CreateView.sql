
--�Զ����������콨�������ͼ������־���ܲɱ�
--��һ��,��̬���ɴ�����ͼ��sql���

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AutoCreateViewSql]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[AutoCreateViewSql]
GO
create proc AutoCreateViewSql
(
   @strCode nvarchar(10),
   @strTableName nvarchar(20)     
        
)
as   
	   if   exists (select *  from sys.objects where type='U' and name=@strTableName+convert(varchar(100),GETDATE(),112))
		   begin    
				 if not exists (select * from BFT_DataRight where strCode=@strCode) --��������ڼ�¼��������һ��                   
					 insert into BFT_DataRight(strCode,strContent) values(@strCode,'create view [dbo].[View'+@strTableName+'] as select * from '+@strTableName+convert(varchar(100),GETDATE(),112))
				 else
                 if((select LEN(strContent) from  BFT_DataRight where strCode=@strCode) = 0 )--���sql�ֶ�û��ֵ����ֵ                      
				     update BFT_DataRight set strContent='create view [dbo].[View'+@strTableName+'] as select * from '+@strTableName+convert(varchar(100),GETDATE(),112)
				 else --�����Ժ�ÿ���Զ�׷��sql            
				     update BFT_DataRight set strContent=Replace(strContent,'create','alter')+'    union all select * from '+@strTableName+convert(varchar(100),GETDATE(),112)
	          
		   end                                                                                                                                                                                                 
go





--�ڶ�������̬������ͼ
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AutoCreateView]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].AutoCreateView
GO

create proc AutoCreateView
(
   @strCode nvarchar(10),
   @strSql nvarchar(max)
)
as
   begin
       set @strSql = (select strContent from BFT_DataRight where strCode=@strCode)      
       exec (@strsql)  
   end 


--�½���ҵ��ʱ�����������裬��һ������ִ������Ĵ洢���̣��ڶ�������ִ���������䣬����ִ�ж��������ݰ���ı���������


--exec AutoCreateViewSql '001','JC_MC'
--go

--exec AutoCreateView '001',''
--go











