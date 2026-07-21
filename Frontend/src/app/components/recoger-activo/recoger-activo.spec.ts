import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RecogerActivo } from './recoger-activo';

describe('RecogerActivo', () => {
  let component: RecogerActivo;
  let fixture: ComponentFixture<RecogerActivo>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RecogerActivo],
    }).compileComponents();

    fixture = TestBed.createComponent(RecogerActivo);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
