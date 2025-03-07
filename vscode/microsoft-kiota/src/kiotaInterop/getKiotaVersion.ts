import * as vscode from "vscode";
import * as rpc from "vscode-jsonrpc/node";

import { connectToKiota } from ".";

export function getKiotaVersion(context: vscode.ExtensionContext, kiotaOutputChannel: vscode.LogOutputChannel): Promise<string | undefined> {
  return connectToKiota<string>(context, async (connection) => {
    const request = new rpc.RequestType0<string, void>("GetVersion");
    const result = await connection.sendRequest(request);
    if (result) {
      const version = result.split("+")[0];
      if (version) {
        kiotaOutputChannel.info(`kiota: ${version}`);
        return version;
      }
    }
    kiotaOutputChannel.error(`kiota: ${vscode.l10n.t('not found')}`);
    kiotaOutputChannel.show();
    return undefined;
  });
};