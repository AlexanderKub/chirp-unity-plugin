#import <Foundation/Foundation.h>
#import <ChirpConnect/ChirpConnect.h>

@interface ChirpPlugin : NSObject
@property (nonatomic) ChirpConnect *connect;

+ (instancetype) sharedChirpPlugin;
- (NSError *)InitSDKWithKey:(NSString *)key andSecret: (NSString *)secret andConfig: (NSString *)config;
- (NSError *)StartSDK;
- (NSError *)StopSDK;
- (NSError *)SendData:(NSData *) payload;
@end
