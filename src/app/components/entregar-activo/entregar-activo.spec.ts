import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EntregarActivo } from './entregar-activo';

describe('EntregarActivo', () => {
  let component: EntregarActivo;
  let fixture: ComponentFixture<EntregarActivo>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EntregarActivo],
    }).compileComponents();

    fixture = TestBed.createComponent(EntregarActivo);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
