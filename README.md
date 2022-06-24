# protobuf-perf
Performance Investigations

## Results from 24. June

![image](https://user-images.githubusercontent.com/10084630/175491542-88e7c2dd-d857-458f-897a-d21bdfc765c2.png)

![image](https://user-images.githubusercontent.com/10084630/175491627-cd8611dc-c6e7-49f1-997c-d3d9e0dbebe1.png)

It looks growing now (ThreadLocal RuntimeTypeModel, but with all the locks inside)
![image](https://user-images.githubusercontent.com/10084630/175491885-e520d931-64a7-4381-bfdc-0963ddb7a2fe.png)

The same test with 4 Mio objects:

![image](https://user-images.githubusercontent.com/10084630/175501241-2a7985a1-2719-49c9-9116-80fb9a264205.png)

![image](https://user-images.githubusercontent.com/10084630/175501455-caad4e04-a13f-4f0e-ab48-fe0a46dc6daa.png)

