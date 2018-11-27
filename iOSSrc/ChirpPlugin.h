#import <Foundation/Foundation.h>
#import <ChirpConnect/ChirpConnect.h>

@interface ChirpPlugin : NSObject
@property (nonatomic) ChirpConnect *connect;

+ (instancetype) sharedChirpPlugin;
- (void)InitSDKWithKey:(NSString *)key andSecret: (NSString *)secret andConfig: (NSString *)config;
- (void)StartSDK;
- (void)StopSDK;
- (void)SendData:(NSData *) payload;
@end
