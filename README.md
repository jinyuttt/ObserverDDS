# ObserverNet
网络订阅发布模型
 本程序采用组播方式寻找节点地址  
通过UDP，TCP发送数据数据  
TCP已经避免粘包；采用最大努力投  
NetPublisher为发布类，提供类单例；NetSubscriber为订阅类，提供类单例  

2019-12  
  1.修改线程使用，将主要流程Task修该Thread，防止线程占用无法完成主要工作  
  2.修改socket参数  
