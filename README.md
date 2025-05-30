导航系统
车载导航系统是在城市中驾车的好帮手，不仅能够计算出到达目的地的最优路线，而且可以显示当前位置附近的一些附加信息，比如附近的加油站，餐馆等等。请设计一款导航软件，实现导航系统的核心功能。
目标用户：汽车司机。
数据配置：导航系统中的地图可以抽象为一个图，地点信息和路径信息可以抽象为图中的顶点和边。请设计算法，来产生模拟的地图数据：
a)        随机产生N个二维平面上的顶点（N>=10000），每个顶点对应地图中的一个地点
b)        对于每个地点x，随机建立若干个连边到x附近的地点。每条连边代表一条路径，路径的长度等于边的两个顶点之间的二维坐标距离。
c)        模拟数据必须保证，产生的图是一个连通图，并且道路之间不应有不合理的交叉。

功能要求：
F1. 地图显示功能。输入一个坐标，显示距离该坐标最近的100个顶点以及相关联的边。关于如何用vc++画图，或java画图，请在baidu或google上查找相关方法，或者借阅相关的参考书。
F2. 地图缩放功能。提示：地图缩的越小，屏幕上显示的点数就越多，但是太多的点，会看不清楚。所以可以考虑只选择一个单元区域内只显示一个代表的点。
F2. 任意指定两个地点A和B，能计算出A到B的最短路径。并将该最短路径经过的顶点以及连边显示出来。
F3. 模拟车流。请为每条连边增加两个属性：车容量v（饱和状态下这条路所能容纳的汽车的数量）、当前在这条路上的车辆数目n（n>v时为超负荷运作）。假设该路的长度为L，则该路的通行时间可模拟为cLf(n/v)，其中c是常数；f(x)是一个分段函数, x小于等于某个常数时，f(x) = 1，当x大于该常数时，f(x) = 1+ex。每条道路的车容量v和道路的长度L为预先指定的固定参数。请模拟产生汽车在地图中行驶，为简化模型假设在同一条路上每架汽车穿越该路的时间均等于cLf(n/v)。要求实现模拟车流的动态变化，任意时刻，给定一个坐标，能在界面上显示该坐标附近的所有路径，并动态显示各个路径上的车流量的大小（可用不同颜色或其他方法区分车流量的大小级别）。
F4. 综合考虑路况的最短路径。任意时刻，指定两个地点A和B，能根据当前的路况，计算出从A到B最短行车时间，以及相应的最佳路径，并在界面上，将该最短路径经过的顶点以及连边显示出来。
