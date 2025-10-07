import { TestBed } from '@angular/core/testing';
import { appConfig } from './app.config';
import { ApplicationConfig } from '@angular/core';

describe('appConfig', () => {
  it('should provide application configuration', () => {
    expect(appConfig).toBeDefined();
    expect(appConfig.providers).toBeDefined();
    expect(Array.isArray(appConfig.providers)).toBe(true);
  });

  it('should configure routing', () => {
    const config = appConfig as ApplicationConfig;
    expect(config.providers.length).toBeGreaterThan(0);
  });
});
