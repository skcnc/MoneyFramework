class ConfigureContext:
   def GetConfigBatchTradeList(self):
       lines = list(open("F:\\workbench\\suncheng\\金融套利系统\\Stork_Future_TaoLi\\ConfigFiles\\BatchTradeList.txt"))
       return lines;