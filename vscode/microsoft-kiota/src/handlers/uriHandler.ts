import * as vscode from 'vscode';
import TelemetryReporter from '@vscode/extension-telemetry';

import { OpenApiTreeProvider } from "../providers/openApiTreeProvider";
import { validateDeepLinkQueryParams } from '../utilities/deep-linking';
import { openTreeViewWithProgress } from '../utilities/progress';
import { setDeepLinkParams, getDeepLinkParams } from './deepLinkParamsHandler';

export class UriHandler {
  constructor(private context: vscode.ExtensionContext, private openApiTreeProvider: OpenApiTreeProvider) { }

  async handleUri(uri: vscode.Uri) {
    if (uri.path === "/") {
      return;
    }
    const queryParameters = this.getQueryParameters(uri);
    if (uri.path.toLowerCase() === "/opendescription") {
      let [params, errorsArray] = validateDeepLinkQueryParams(queryParameters);
      setDeepLinkParams(params);

      const reporter = new TelemetryReporter(this.context.extension.packageJSON.telemetryInstrumentationKey);
      reporter.sendTelemetryEvent("DeepLink.OpenDescription initialization status", {
        "queryParameters": JSON.stringify(queryParameters),
        "validationErrors": errorsArray.join(", ")
      });

      let deepLinkParams = getDeepLinkParams();
      if (deepLinkParams.descriptionurl) {
        await openTreeViewWithProgress(() => this.openApiTreeProvider.setDescriptionUrl(deepLinkParams.descriptionurl!));
        return;
      }
    }
    void vscode.window.showErrorMessage(
      vscode.l10n.t("Invalid URL, please check the documentation for the supported URLs")
    );
  }

  private getQueryParameters(uri: vscode.Uri): Record<string, string> {
    const query = uri.query;
    if (!query) {
      return {};
    }
    const queryParameters = (query.startsWith('?') ? query.substring(1) : query).split("&");
    const parameters = {} as Record<string, string>;
    queryParameters.forEach((element) => {
      const keyValue = element.split("=");
      parameters[keyValue[0].toLowerCase()] = decodeURIComponent(keyValue[1]);
    });
    return parameters;
  }
}