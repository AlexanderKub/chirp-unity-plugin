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

- (void)InitSDKWithKey:(NSString *)key andSecret: (NSString *)secret andConfig: (NSString *)config {
    if (self.connect != nil) {
        [self.connect stop];
        self.connect = nil;
    }
    
    self.connect = [[ChirpConnect alloc] initWithAppKey:key andSecret:secret];
    NSLog(@"%@", [self.connect version]);
    NSError *error = [self.connect setConfig:config];
    if (error) {
        NSLog(@"Error when setting config: %@", error);
        return;
    }
    
    [self.connect setStateUpdatedBlock:^(CHIRP_CONNECT_STATE oldState, CHIRP_CONNECT_STATE newState) {
        /*------------------------------------------------------------------------------
         * stateChangedBlock is called when the SDK changes state.
         *----------------------------------------------------------------------------*/
        dispatch_async (dispatch_get_main_queue(), ^ {
            /*switch (newState) {
             case CHIRP_CONNECT_STATE_NOT_CREATED:
             NSLog(@"Not initialised"); break;
             case CHIRP_CONNECT_STATE_STOPPED:
             NSLog(@"Stopped"); break;
             case CHIRP_CONNECT_STATE_PAUSED:
             NSLog(@"Paused"); break;
             case CHIRP_CONNECT_STATE_RUNNING:
             NSLog(@"Running"); break;
             case CHIRP_CONNECT_STATE_SENDING:
             NSLog(@"Sending"); break;
             case CHIRP_CONNECT_STATE_RECEIVING:
             NSLog(@"Receiving"); break;
             }*/
            OnChangeStateEvent(newState);
        });
    }];
    
    [self.connect setSystemVolumeChangedBlock:^(float volume) {
        /*------------------------------------------------------------------------------
         * systemVolumeChangedBlock is called when the user changes the hardware volume.
         *----------------------------------------------------------------------------*/
        NSLog(@"Volume changed: %.4f", volume);
    }];
    
    [self.connect setAuthenticatedBlock:^(NSError * _Nullable error) {
        NSLog(@"Authenticated (%@)", error ? error.description: @"OK");
    }];
    
    [self.connect setSendingBlock:^(NSData * _Nonnull data) {
        /*------------------------------------------------------------------------------
         * sendingBlock is called when a send event begins.
         * The data argument contains the payload being sent.
         *----------------------------------------------------------------------------*/
        //NSLog(@"Sending data: %@", data);
    }];
    
    [self.connect setSentBlock:^(NSData * _Nonnull data) {
        /*------------------------------------------------------------------------------
         * sentBlock is called when a send event has completed.
         * The data argument contains the payload that was sent.
         *----------------------------------------------------------------------------*/
        //NSLog(@"Sent data: %@", data);
        OnSentEvent((char *)[data bytes]);
    }];
    
    [self.connect setReceivingBlock:^(NSUInteger channel) {
        /*------------------------------------------------------------------------------
         * receivingBlock is called when a receive event begins.
         * No data has yet been received.
         *----------------------------------------------------------------------------*/
        //NSLog(@"Receiving on channel %lu", (unsigned long) channel);
    }];
    
    [self.connect setReceivedBlock:^(NSData * _Nonnull data, NSUInteger channel) {
        /*------------------------------------------------------------------------------
         * receivedBlock is called when a receive event has completed.
         * If the payload was decoded successfully, it is passed in data.
         * Otherwise, data is null.
         *----------------------------------------------------------------------------*/
        if (data) {
            //NSLog(@"Received data: %@ on channel %lu", data, (unsigned long) channel);
            NSString *str = [[NSString alloc] initWithBytes:[data bytes] length:[data length] encoding:NSASCIIStringEncoding];
            const char *t = [str UTF8String];
            OnRecieveEvent(t);
        } else {
            //NSLog(@"Decode failed.");
            OnRecieveEvent(nil);
        }
    }];
}

- (void)StartSDK {
    if (self.connect == nil) {
        return;
    }
    NSError *error = [self.connect start];
    if (error) {
        NSLog(@"Error starting engine: %@", error);
    }
}

- (void)StopSDK {
    if (self.connect == nil) {
        return;
    }
    [self.connect stop];
}

- (void)SendData:(NSData *)payload {
    if (self.connect == nil) {
        return;
    }
    NSError *error = [self.connect send:payload];
    if (error) {
        NSLog(@"Error sending data: %@", error);
    }
}
@end

extern "C" {
    void ChirpInitSDK(char *key, char *secret, char *config) {
        [[ChirpPlugin sharedChirpPlugin] InitSDKWithKey:[NSString stringWithUTF8String:key]
                                              andSecret:[NSString stringWithUTF8String:secret]
                                              andConfig:[NSString stringWithUTF8String:config]];
    }
    
    void ChirpStartSDK() {
        UnitySetAudioSessionActive(0);
        [[ChirpPlugin sharedChirpPlugin] StartSDK];
    }
    
    void ChirpStopSDK() {
        [[ChirpPlugin sharedChirpPlugin] StopSDK];
        UnitySetAudioSessionActive(1);
    }
    
    void ChirpSendData(int length, const char* payload) {
        NSData *nspayload = [NSData dataWithBytes:payload length:length];
        [[ChirpPlugin sharedChirpPlugin] SendData:nspayload];
    }
    
    void RegisterPluginHandlers(MonoPStateChangeDelegate changeStateDelegate,
                                MonoPDataDelegate recieveDataDelegate,
                                MonoPDataDelegate sentDataDelegate) {
        _changeStateDelegate = changeStateDelegate;
        _recieveDataDelegate = recieveDataDelegate;
        _sentDataDelegate = sentDataDelegate;
    }
}
