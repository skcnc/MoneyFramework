class ConfigureContext:
   def GetConfigBatchTradeList(self):
       lines = list(open("F:\\workbench\\suncheng\\ConfigFiles\\BatchTradeList.txt"))
       return lines;