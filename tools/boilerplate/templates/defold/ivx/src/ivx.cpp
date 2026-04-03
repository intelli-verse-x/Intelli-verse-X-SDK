// Minimal dummy native extension for Defold
#define EXTENSION_NAME ivx
#define LIB_NAME "ivx"
#define MODULE_NAME "ivx"

#include <dmsdk/sdk.h>

static dmExtension::Result AppInitializeivx(dmExtension::AppParams* params) {
    return dmExtension::RESULT_OK;
}

static dmExtension::Result Initializeivx(dmExtension::Params* params) {
    return dmExtension::RESULT_OK;
}

static dmExtension::Result AppFinalizeivx(dmExtension::AppParams* params) {
    return dmExtension::RESULT_OK;
}

static dmExtension::Result Finalizeivx(dmExtension::Params* params) {
    return dmExtension::RESULT_OK;
}

DM_DECLARE_EXTENSION(EXTENSION_NAME, LIB_NAME, AppInitializeivx, AppFinalizeivx, Initializeivx, 0, 0, Finalizeivx)
