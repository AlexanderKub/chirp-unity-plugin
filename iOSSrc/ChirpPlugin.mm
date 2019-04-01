#import "ChirpPlugin.h"
#import <ChirpConnect/ChirpConnect.h>

typedef void (*MonoPDataDelegate)(const char* data);
typedef void (*MonoPStateChangeDelegate)(chirp_connect_state_t state);

static MonoPStateChangeDelegate _changeStateDelegate = NULL;
static MonoPDataDelegate _recieveDataDelegate = NULL;
static MonoPDataDelegate _sentDataDelegate = NULL;

void OnRecieveEvent(const char* data) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if(_recieveDataDelegate != NULL) {
            _recieveDataDelegate(data);
        }
    });
}

void OnSentEvent(const char* data) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if(_sentDataDelegate != NULL) {
            _sentDataDelegate(data);
        }
    });
}

void OnChangeStateEvent(chirp_connect_state_t state) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if(_changeStateDelegate != NULL) {
            _changeStateDelegate(state);
        }
    });
}

NSError *NotCreatedError = [NSError errorWithDomain:@"io.chirp.plugin.error" code:404 userInfo:@{@"error": @"Not yet created connect"}];

@implementation ChirpPlugin

+ (instancetype) sharedChirpPlugin {
    static id instance = nil;
    
    @synchronized(self) {
        if (instance == nil) {
            instance = [[self alloc] init];
        }
    }
    
    return instance;
}

- (NSError *)InitSDKWithKey:(NSString *)key andSecret: (NSString *)secret andConfig: (NSString *)config {
    if (self.connect != nil) {
        [self.connect stop];
        self.connect = nil;
    }
    
    self.connect = [[ChirpConnect alloc] initWithAppKey:key andSecret:secret];
    NSError *error = [self.connect setConfig:config];
    if (error) {
        return error;
    }
    
    [self.connect setStateUpdatedBlock:^(CHIRP_CONNECT_STATE oldState, CHIRP_CONNECT_STATE newState) {
        /*------------------------------------------------------------------------------
         * stateChangedBlock is called when the SDK changes state.
         *----------------------------------------------------------------------------*/
        dispatch_async (dispatch_get_main_queue(), ^ {
            OnChangeStateEvent(newState);
        });
    }];
    
    [self.connect setSystemVolumeChangedBlock:^(float volume) {
        /*------------------------------------------------------------------------------
         * systemVolumeChangedBlock is called when the user changes the hardware volume.
         *----------------------------------------------------------------------------*/
    }];
    
    [self.connect setAuthenticatedBlock:^(NSError * _Nullable error) {
    }];
    
    [self.connect setSendingBlock:^(NSData * _Nonnull data, NSUInteger channel) {
        /*------------------------------------------------------------------------------
         * sendingBlock is called when a send event begins.
         * The data argument contains the payload being sent.
         *----------------------------------------------------------------------------*/
    }];
    
    [self.connect setSentBlock:^(NSData * _Nonnull data, NSUInteger channel) {
        /*------------------------------------------------------------------------------
         * sentBlock is called when a send event has completed.
         * The data argument contains the payload that was sent.
         *----------------------------------------------------------------------------*/
        OnSentEvent((char *)[data bytes]);
    }];
    
    [self.connect setReceivingBlock:^(NSUInteger channel) {
        /*------------------------------------------------------------------------------
         * receivingBlock is called when a receive event begins.
         * No data has yet been received.
         *----------------------------------------------------------------------------*/
    }];
    
    [self.connect setReceivedBlock:^(NSData * _Nonnull data, NSUInteger channel) {
        /*------------------------------------------------------------------------------
         * receivedBlock is called when a receive event has completed.
         * If the payload was decoded successfully, it is passed in data.
         * Otherwise, data is null.
         *----------------------------------------------------------------------------*/
        if (data) {
            NSString *str = [[NSString alloc] initWithBytes:[data bytes] length:[data length] encoding:NSASCIIStringEncoding];
            const char *t = [str UTF8String];
            OnRecieveEvent(t);
        } else {
            OnRecieveEvent(nil);
        }
    }];
    
    return nil;
}

- (NSError *)StartSDK {
    if (self.connect == nil) {
        return NotCreatedError;
    }
    NSError *error = [self.connect start];
    if (error) {
        return error;
    }
    return nil;
}

- (NSError *)StopSDK {
    if (self.connect == nil) {
        return NotCreatedError;
    }
    NSError *error = [self.connect stop];
    if (error) {
        return error;
    }
    return nil;
}

- (NSError *)SendData:(NSData *)payload {
    if (self.connect == nil) {
        return NotCreatedError;
    }
    NSError *error = [self.connect send:payload];
    if (error) {
        return error;
    }
    return nil;
}
@end

extern "C" {
    int ChirpInitSDK(char *key, char *secret, char *config) {
        NSError *error = [[ChirpPlugin sharedChirpPlugin] InitSDKWithKey:[NSString stringWithUTF8String:key] andSecret:[NSString stringWithUTF8String:secret] andConfig:[NSString stringWithUTF8String:config]];
        return error ? (int)[error code] : 0;
    }
    
    int ChirpStartSDK() {
        UnitySetAudioSessionActive(0);
        NSError *error = [[ChirpPlugin sharedChirpPlugin] StartSDK];
        return error ? (int)[error code] : 0;
    }
    
    int ChirpStopSDK() {
        NSError *error = [[ChirpPlugin sharedChirpPlugin] StopSDK];
        UnitySetAudioSessionActive(1);
        return error ? (int)[error code] : 0;
    }
    
    int ChirpSendData(int length, const char* payload) {
        NSData *nspayload = [NSData dataWithBytes:payload length:length];
        NSError *error = [[ChirpPlugin sharedChirpPlugin] SendData:nspayload];
        return error ? (int)[error code] : 0;
    }
    
    void RegisterPluginHandlers(MonoPStateChangeDelegate changeStateDelegate,
                                MonoPDataDelegate recieveDataDelegate,
                                MonoPDataDelegate sentDataDelegate) {
        _changeStateDelegate = changeStateDelegate;
        _recieveDataDelegate = recieveDataDelegate;
        _sentDataDelegate = sentDataDelegate;
    }
}
