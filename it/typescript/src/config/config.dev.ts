import { globalConfig } from './config.global';
import { IConfig } from './config.interface';

export const config: IConfig = {
  ...globalConfig,

  apiUrl: 'https://a-development-url',
};
